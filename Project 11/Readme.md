# Projeto 11 — Disparos de WhatsApp confiáveis

Duas correções no envio automático de WhatsApp do TV Player: um **incidente em produção** em que clientes receberam a **mesma mensagem três vezes na mesma manhã**, e um **template novo com imagem no cabeçalho** que o provedor aceitava com `201`, mas que nunca chegava ao celular.

> **Continuação do Projeto 8.** O `VerificadorDeDemonstracoes` — o serviço em segundo plano que avisa o cliente de que a demonstração está acabando — é o mesmo do **Projeto 8**, que ficou preservado lá como o "antes". Aqui está o que aconteceu quando ele encontrou o mundo real, e como o canal de WhatsApp (já migrado do ChatPro para a **DigiSac**) ganhou um formato de template novo.

---

## Parte 1 — O incidente: a mesma mensagem três vezes

### O sintoma

Clientes receberam **"sua demonstração está acabando"** três vezes na mesma manhã: **10:13, 10:24 e 10:50**.

### A investigação: os horários eram a impressão digital

O serviço arma um `System.Threading.Timer` com **5 minutos de espera inicial** e **período de 20 minutos**:

```csharp
_timer = new Timer(ExecutarVerificacao, null, TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(20));
```

Subtraindo 5 minutos de cada envio (10:13:58, 10:24:27, 10:50:32) chega-se a **10:08, 10:19 e 10:45**. Intervalos de 10 e 26 minutos são impossíveis para um único processo com período de 20 minutos. Não eram três execuções de um processo — eram **o primeiro tique de três processos diferentes**. O IIS reciclou a aplicação três vezes dentro da janela das 10h.

### A causa: a trava morava na memória

```csharp
private static bool _jaExecutouHoje = false;
```

A pergunta "já avisei este cliente hoje?" era respondida por um **campo estático**. `static` significa "um por processo" — e processo, no IIS, sobe e desce o tempo todo (idle timeout, reciclagem do pool, deploy). A cada subida, `_jaExecutouHoje` nascia `false` de novo, ainda dentro da hora das 10h, e o disparo saía outra vez.

Agravante: o callback do `Timer` é `async void` (a API só aceita `void Callback(object state)`), então qualquer exceção sem `try/catch` derrubaria o processo — mais um motivo de reinício.

### A correção

| Antes | Depois |
|---|---|
| `static bool _jaExecutouHoje` na memória | coluna **`dbo.Terminal.DataAvisoPenultimoDia`** (`datetime NULL`) no banco |
| "Já executei desde que este processo subiu?" | "Já avisei este terminal **nesta instalação**?" |
| Marca depois do envio (se marcava) | **Marca antes de enviar** — garantia de *no máximo uma vez* |
| Erro de banco → comportamento indefinido | **Falha fechada**: erro na consulta = ninguém recebe |
| Mesmo usuário podia sair duas vezes (empresa com 2 terminais) | `GroupBy(u => u.ID)` — uma mensagem por pessoa |

**1. A marca persistida, comparável com o ciclo.** Uma data, não um booleano:

```sql
SELECT ID FROM dbo.Terminal
 WHERE DataAvisoPenultimoDia IS NOT NULL
   AND DataAvisoPenultimoDia >= DataInstalacao
   AND ID IN (@id0, @id1, @id2)
```

O `>= DataInstalacao` foi um adendo da revisão: sem ele, um terminal **reinstalado** (que recomeça a contagem de dias) nunca mais receberia o aviso — trocar "mandou três vezes" por "nunca mais manda" não é consertar. Com `datetime` em vez de `bit`, a correção coube numa linha, sem rotina de limpeza noturna.

**2. SQL parametrizado com lista de tamanho variável.** Os IDs não entram no texto do SQL; entram os **nomes** dos parâmetros, gerados com a sobrecarga do `Select` com índice:

```csharp
List<string> parametros = terminalIds.Select((id, indice) => "@id" + indice).ToList();
string sql = $"... AND ID IN ({string.Join(", ", parametros)})";
...
for (int i = 0; i < terminalIds.Count; i++)
    command.Parameters.AddWithValue(parametros[i], terminalIds[i]);
```

ADO.NET direto, e não EF, de propósito: declarar a coluna na entidade `Terminal` (usada por dezenas de consultas) faria **todas** as consultas quebrarem se o código subisse antes do `ALTER TABLE`. Com ADO.NET, o raio de alcance é zero.

**3. Falhar fechado.**

```csharp
catch (Exception ex)
{
    VerificadorDeDemonstracoesLogger.Log($"Erro ao consultar terminais ja avisados: {ex.Message}. Nenhum aviso sera enviado nesta execucao.");
    return terminalIds;
}
```

Se a consulta de "já avisados" falha, o método devolve **todos** os candidatos como já avisados. Na dúvida, silêncio. Efeito colateral útil: publicar o código **antes** de rodar o `ALTER` não manda nada a ninguém — erro de ordem de deploy vira "não mandou", nunca "mandou três vezes".

**4. Marcar antes de enviar.** `await MarcarTerminaisAvisadosAsync(...)` vem antes do laço de envio. Se a marcação falhar, o `await` relança a exceção e nenhuma mensagem sai. A troca é assumida: se o envio falhar depois da marcação, aquele cliente não recebe nova tentativa — mensagem repetida incomoda o cliente e queima a reputação do número no WhatsApp.

> **A lição:** estado que decide se uma ação **visível para fora** acontece (mensagem, cobrança, e-mail) não pode morar na memória do processo. O outro serviço do mesmo diretório, `DisparoDemonstracao` (Projeto 8), atravessou os mesmos reinícios sem duplicar nada — porque a deduplicação dele sempre esteve no banco (`AND DisparoWhatsApp IS NULL`).

---

## Parte 2 — Template de WhatsApp com imagem no cabeçalho (DigiSac)

### O sintoma

O teste do template novo `account_success9` passava pela DigiSac com **`201 Created`**, mas a mensagem nunca chegava. O log de diagnóstico do serviço mostrou o veredito da Meta, que chega **depois** do `201`:

```
(#132012) Parameter format does not match format in the created template
header: Format mismatch, expected IMAGE, received UNKNOWN
```

O template aprovado tinha `HEADER` com `format: IMAGE`, `BODY` com `{{1}}` e dois botões quick-reply. O código mandava só o `body`.

### Por que o erro apareceu (em vez de passar mentindo)

A cadeia é DigiSac → Gupshup → Meta, e a Meta valida **de forma assíncrona**. O `DisparosDigisac` não confia no `201`: depois do `POST`, faz um **poll de status** (até 6 tentativas, 600 ms entre elas) em `GET /api/v1/messages/{id}`, lendo `data.ack`:

- `"error"` → falha, com a mensagem e o `details` reais da Meta;
- `>= 1` → entregue/lido;
- `0` até o tempo-limite → aceito (entrega pendente).

Sem esse poll, o `201` teria sido registrado como sucesso.

### A solução: um membro novo no contrato

```csharp
public interface IDisparoMensagem
{
    Task<EnvioMensagemResultado> EnviarMensagemAsync(string number, string message);
    Task<EnvioMensagemResultado> EnviarTemplateAsync(string number, string templateId, bool comBotaoCopyCode, params string[] parametros);
    Task<EnvioMensagemResultado> EnviarTemplateComBotaoUrlAsync(string number, string templateId, string urlButtonParam, int urlButtonIndex = 0, params string[] parametrosBody);

    Task<EnvioMensagemResultado> EnviarTemplateComImagemAsync(string number, string templateId, string urlImagem, params string[] parametrosBody);
}
```

Os serviços do site recebem `IDisparoMensagem` por injeção de dependência — um método que existisse só na classe seria invisível para eles. Acrescentar membro a interface quebra todas as implementações; aqui havia só uma (`DisparosDigisac`).

### A armadilha evitada: parâmetro opcional antes de `params`

A tentação era esticar o método existente:

```csharp
EnviarTemplateAsync(string number, string templateId, bool comBotaoCopyCode, string urlImagem = null, params string[] parametros);
```

E a chamada que já está em produção é:

```csharp
_disparoMensagem.EnviarTemplateAsync(numeroDestino, templateId, false, nomeCliente, nomeEmpresa);
```

Argumentos posicionais preenchem os parâmetros **em ordem**. `nomeCliente` cairia em `urlImagem` (é `string`, o compilador aceita calado) e só `nomeEmpresa` iria para o corpo. **Compila, não dá aviso, e manda a mensagem errada.** Repare que `int urlButtonIndex = 0` no método vizinho não sofre disso: passar `"João"` ali dá erro CS1503. **Tipo diferente protege; tipo igual engole calado.** Um método com nome próprio elimina a armadilha e diz, no nome, o formato do template que serve.

### O JSON sem classe: tipo anônimo

```csharp
var componentes = new System.Collections.Generic.List<object>
{
    new
    {
        type = "header",
        parameters = new[] { new { type = "image", image = new { link = urlImagem } } }
    }
};
```

O destino é JSON e nada mais. A Newtonsoft serializa o tipo anônimo usando o nome da propriedade como chave — os nomes em minúsculo quebram o `PascalCase` de propósito, porque são as chaves que a API da Meta exige:

```json
{"type":"header","parameters":[{"type":"image","image":{"link":"https://..."}}]}
```

A lista é de `object` porque o `header` e o `body` geram **tipos anônimos diferentes** (um tem `parameters` de `{type, image}`, o outro de `{type, text}`), sem parentesco além de `object`.

### Guarda local em vez de falha remota

Sem `urlImagem`, o método retorna erro **antes** de qualquer HTTP. Transforma "falha remota e atrasada" (201 → poll → rejeição da Meta segundos depois) em "falha local e imediata", com uma mensagem que nomeia o parâmetro.

### De onde sai a URL da imagem

A imagem enviada no cadastro do template é **só o exemplo da aprovação**. Em **cada disparo**, o cabeçalho precisa apontar para a mídia de novo — e quem baixa é o servidor da Meta, que não enxerga arquivo local. A solução não exigiu código: o próprio site já serve `wwwroot` na raiz via `UseStaticFiles`, então `wwwroot/img/ImagemDeTemplate.jpeg` (175 KB, dentro do limite de 5 MB da Meta) vira uma URL pública configurada no `appsettings.json`.

### Testes

**Unitários (`DisparosDigisacTests`)** — sem rede. Um `FakeHandler : HttpMessageHandler` (herança + `override` de `SendAsync`) captura o corpo do `POST` e simula o status do poll. Os dois testes novos:

- `EnviarTemplateComImagem_MontaHeaderComImagemEBody` — lê o JSON que **de fato saiu** pelo `HttpClient` e confere `header` → `type: image` → `image.link`, o `body`, e que **não** há componente `button` (os botões do template são quick-reply estáticos; mandar `button` seria outro erro da Meta).
- `EnviarTemplateComImagem_SemUrlImagem_RetornaErroSemChamarHttp` — prova a guarda pelo lado que importa: `PostCount == 0`.

Resultado: **15 aprovados, 0 falhas** (13 existentes + 2 novos).

**Integração (`DisparosDigisacIntegrationTests`)** — disparam **de verdade** contra a DigiSac, um teste por template, espelhando a chamada que a produção faz. Proteções:

- `[SkippableFact]` + `Skip.IfNot(DIGISAC_INTEGRATION == "1")`: no `dotnet test` do dia a dia, todos ficam **Skipped**;
- destino **fixo numa constante** (o número do dono do projeto), nunca lido de config ou banco — não existe caminho que dispare para cliente real;
- configuração vazia **falha explicitamente** em vez de mandar `hsmId` vazio.

---

## Resultado

| | Antes | Depois |
|---|---|---|
| Trava de "já avisei" | campo `static` em memória | coluna `datetime` em `dbo.Terminal` |
| Sobrevive a reciclagem do IIS | não (3 envios em 37 min) | sim |
| Terminal reinstalado | — | volta a ser avisado (marca comparada com `DataInstalacao`) |
| Falha ao consultar o banco | indefinido | ninguém recebe (falha fechada) |
| Mesma pessoa, 2 terminais no mesmo dia | 2 mensagens | 1 mensagem |
| Template com imagem | `201`, mas a Meta recusava (`#132012`) | método dedicado com `header` de imagem |
| `201` do provedor | — | nunca é lido como "entregue": poll do status real |
| Testes unitários do cliente DigiSac | 13 | 15 (100% aprovados) |

### Status honesto

- **Parte 1:** compila (0 erros); o `ALTER TABLE` e o `SELECT`/`UPDATE` parametrizados foram executados contra o banco sem erro. A validação definitiva é observar o disparo das 10h após a publicação — o serviço não pode ser exercitado localmente sem mandar WhatsApp a cliente real.
- **Parte 2:** testes unitários verdes. O teste de integração do template com imagem depende da imagem estar publicada (antes do deploy, a URL respondia `302` para uma página HTML, que a Meta recusaria).

## Tecnologias e conceitos

- **ASP.NET Core** (.NET) · C# · **SQL Server**
- **`IHostedService`** + **`System.Threading.Timer`** · `async void` em callback (e por que é perigoso)
- **ADO.NET** (`SqlConnection`, `SqlCommand`, `SqlDataReader`) · **SQL parametrizado** com `IN` de tamanho variável
- **Idempotência / *at-most-once*** · **falhar fechado** · estado durável × estado em memória
- **Interfaces e DIP** (`IDisparoMensagem`) · **tipos anônimos** → JSON (**Newtonsoft.Json**)
- **API DigiSac** (WhatsApp Business via Gupshup/Meta) · templates HSM com cabeçalho de mídia · poll de confirmação de entrega
- **xUnit** · **Xunit.SkippableFact** · `HttpMessageHandler` falso para testar HTTP sem rede

## Estrutura

```
DisparosWhatsApp/
├─ Api/Disparos/
│  └─ VerificadorDeDemonstracoes.cs          → aviso do penúltimo dia; marca persistida + falha fechada
├─ Site/Disparos/
│  ├─ IDisparoMensagem.cs                    → contrato; membro novo EnviarTemplateComImagemAsync
│  └─ DisparosDigisac.cs                     → cliente DigiSac: payloads, POST, log e poll de status
├─ Tests/
│  ├─ DisparosDigisacTests.cs                → unitários com FakeHandler (sem rede)
│  └─ DisparosDigisacIntegrationTests.cs     → disparo real, protegido por DIGISAC_INTEGRATION=1
├─ Sql/
│  └─ AlterTerminal_DataAvisoPenultimoDia.sql → coluna nova em dbo.Terminal (idempotente)
└─ Config/
   └─ appsettings.Digisac.exemplo.json       → chaves usadas, com os valores sensíveis removidos
```

> 📄 O `.sql` foi reescrito a partir da mudança descrita (coluna `datetime` anulável), com guarda `COL_LENGTH` para poder rodar mais de uma vez.

> 🔒 Nenhuma credencial neste diretório: token, `serviceId` e IDs de template da DigiSac, telefones (inclusive o número comercial e o de teste) e identificadores de clientes foram substituídos por `"<REDACTED>"`. Em produção, tudo vem de configuração.
