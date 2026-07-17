# Projeto 9 — Refatoração do fluxo de Demonstração (SRP + DI)

Refatoração de um **controller de 825 linhas** que concentrava todo o fluxo de demonstração do site TV Player (geração e verificação do código, cadastro do cliente, criação da licença, e-mails, WhatsApp e log) em um controller de **199 linhas** que só cuida de HTTP, apoiado em **8 serviços injetados por interface**.

> **Mesmo comportamento, zero mudança de contrato.** Nenhuma rota, payload ou resposta JSON mudou — a refatoração é interna. O código refatorado está em produção.
>
> *(O fluxo em si é o do **Projeto 7**, que ficou preservado aqui como o "antes".)*

## O problema

O `DemonstracaoController` cresceu por acréscimo: cada nova regra (WhatsApp, reenvio, validação de e-mail, log) entrou como mais um bloco dentro da action. O resultado:

- **Uma classe, muitas responsabilidades** — HTTP, regra de negócio, acesso à sessão, e-mail, WhatsApp, licença e log no mesmo arquivo.
- **Dependências escondidas e não substituíveis** — `new EmailHelper()` dentro das actions, `LogVerificacaoHelper` estático, `DateTime.Now` e `new Random()` direto no código. Nada disso aparece no construtor, então nada disso pode ser trocado ou mockado.
- **Lógica duplicada** — as chaves da sessão (`DemonstracaoTemp`, `CodigoVerificacaoTemp`, `TimestampVerificacaoTemp`), o `JsonConvert` e o parse da data eram repetidos em 5 actions.
- **Regras como número mágico** — os 5 minutos de validade e os defaults da licença estavam hard-coded.
- **Praticamente intestável** — para testar "código expirado" era preciso subir um `HttpContext`, uma sessão real e esperar o relógio andar.

## A solução

| Camada | Responsabilidade |
|--------|------------------|
| `DemonstracaoController` | **Só HTTP**: recebe, valida `ModelState`/reCAPTCHA, chama o serviço, devolve JSON/View. |
| `DemonstracaoAppService` | **Orquestra** o fluxo (iniciar verificação, verificar código, reenviar, validar e-mail). |
| `CadastroClienteService` | Cliente novo × existente, criação da licença, e-mail e WhatsApp de boas-vindas. |
| `CodigoVerificacaoService` | Gera o código e decide se ainda é válido. |
| `VerificacaoSessionStore` | Único ponto que conhece a sessão (chaves, JSON, data). |
| `EmailService` | Intenção (`EnviarConfirmacaoCadastro`) no lugar de caminho de template. |
| `DadosService` | Normalização de telefone (DDI) e formatação de nome/empresa. |
| `VerificacaoLogger` | O log estático virou serviço injetável. |
| `IRelogio` | Abstrai `DateTime.Now` — é o que torna a expiração testável. |

Cada serviço é registrado no `Startup` por **interface**, então o controller depende de abstração, não de implementação (**DIP**).

## Os 7 passos

A refatoração foi feita **em etapas pequenas e verificáveis**, uma responsabilidade por vez, mantendo o site funcionando ao fim de cada passo:

1. **`IRelogio`** — tirar `DateTime.Now` do caminho da regra de expiração.
2. **`CodigoVerificacaoService`** — extrair geração/validade do código; trocar `new Random()` por `RandomNumberGenerator` (CSPRNG) e os 5 minutos por configuração.
3. **`VerificacaoSessionStore`** — concentrar o acesso à sessão num contrato só (`Salvar`/`TryObter`/`Limpar`).
4. **`DadosService`** — extrair as regras de telefone e nome, que estavam repetidas.
5. **`EmailService`** — remover o `new EmailHelper()` e o caminho do `.txt` de dentro das actions.
6. **`VerificacaoLogger`** — transformar o helper estático em serviço injetado.
7. **`DemonstracaoAppService` + `CadastroClienteService`** — mover a orquestração e o cadastro para fora do controller.

## Resultado

| | Antes | Depois |
|---|---|---|
| Controller | **825 linhas** | **199 linhas** (−76%) |
| Responsabilidades no controller | 7 | 1 (HTTP) |
| Dependências criadas com `new` / estáticas | 4 (`EmailHelper`, `LogVerificacaoHelper`, `Random`, `DateTime.Now`) | 0 |
| Acesso à sessão | espalhado em 5 actions | 1 classe |
| Validade do código / defaults da licença | hard-coded | `appsettings.json` (Options Pattern) |
| Testável com mock | não | sim (todas as dependências por interface) |

### Ganhos além do tamanho

- **Segurança** — o código de verificação passou a usar `RandomNumberGenerator` no lugar de `Random`, que é previsível.
- **Testabilidade** — com `IRelogio` e `IVerificacaoSessionStore` mockados, "código expirado" vira um teste de milissegundos, sem `HttpContext` nem espera real.
- **Configuração** — mudar a validade do código virou edição de `appsettings.json`, sem recompilar.
- **Troca de fornecedor** — o WhatsApp já vinha atrás de `IDisparoMensagem`; trocar o provedor não toca em nenhum serviço.

## Tecnologias e conceitos

- **ASP.NET Core MVC** (.NET) · C#
- **SOLID** — SRP (cada serviço com um motivo para mudar) e DIP (dependência de interface)
- **Injeção de Dependência** nativa (`IServiceCollection`) — `Scoped` / `Singleton` conforme o estado
- **Options Pattern** (`IOptions<DemonstracaoOptions>`, `SmtpOptions`)
- **Clock abstraction** (`IRelogio`) para tornar tempo testável
- **`RandomNumberGenerator`** (criptograficamente seguro)
- **Refatoração incremental** — passos pequenos, comportamento preservado

## Estrutura

```
RefatoracaoDemonstracao/
├─ Antes/
│  ├─ Controllers/DemonstracaoController.cs   → 825 linhas, tudo junto
│  └─ Helpers/LogVerificacaoHelper.cs         → log estático (dependência escondida)
└─ Depois/
   ├─ Controllers/DemonstracaoController.cs   → 199 linhas, só HTTP
   ├─ Startup.DI.cs                           → registro dos serviços (ponto de composição)
   ├─ Services/
   │  ├─ Abstractions/IRelogio.cs
   │  └─ Demonstracao/
   │     ├─ DemonstracaoAppService.cs         → orquestra o fluxo
   │     ├─ CadastroClienteService.cs         → cadastro + licença + boas-vindas
   │     ├─ CodigoVerificacaoService.cs       → gera/valida o código
   │     ├─ VerificacaoSessionStore.cs        → único dono da sessão
   │     ├─ EmailService.cs                   → e-mails por intenção
   │     ├─ DadosService.cs                   → telefone e nome
   │     └─ VerificacaoLogger.cs              → log injetável
   ├─ Configuration/
   │  ├─ DemonstracaoOptions.cs               → validade do código + defaults da licença
   │  └─ SmtpOptions.cs                       → SMTP fora do código
   └─ Disparos/IDisparoMensagem.cs            → abstração do WhatsApp
```

> 📄 `Startup.DI.cs` é um **recorte ilustrativo** do `ConfigureServices` real (só a parte da demonstração), para deixar o ponto de composição visível sem trazer o `Startup` inteiro do site.

> 🔒 Nenhuma credencial neste diretório: token do WhatsApp, SMTP e conexões vêm todos de configuração.
