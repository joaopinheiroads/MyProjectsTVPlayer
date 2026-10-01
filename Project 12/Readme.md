# Projeto 12 — Manutenção de um painel legado em produção (ASP.NET Web Forms)

Quatro intervenções, em setembro de 2026, no **painel administrativo da TV Player** (`PlayerVideo`): um site **ASP.NET Web Forms em .NET Framework**, com **DevExpress 18.2**, um `.csproj` de **mais de 5.300 linhas** no formato antigo e uma DLL de produção compilada a partir de um fonte que, no começo do trabalho, não estava à mão.

1. O painel **tinha parado de compilar** — e a busca do Instagram, que dependia dele, voltou a funcionar.
2. O build passou a sair **sem erro, mas quebrava em tempo de execução** — provado e corrigido sem subir o site.
3. As **categorias do Canaltech passaram a aparecer na árvore** da programação, como as do UOL, **sem recompilar a DLL**.
4. **Arrastar vários itens selecionados** para a programação passou a adicionar todos (antes ia só um).

> **Nenhum contrato mudou.** O servidor continua recebendo exatamente as mesmas chamadas, o banco grava os mesmos registros e o player instalado nos terminais não percebe nada. Tudo isso foi ao ar e foi conferido em produção.

## O problema

**1. O build que ninguém tentava.** A biblioteca compartilhada `TVPlayerAPI` subiu para o .NET Framework **4.7.2** e renomeou o método que prepara a busca do Instagram (agora no Apify). O serviço Windows acompanhou; o painel ficou em **4.5.2**. Ao tentar compilar, apareceram **quatro camadas de erro**, uma escondendo a outra:

- **`CS0246` em 26 arquivos** — "namespace `TVPlayerAPI` não encontrado". A causa real era um *aviso* perdido no meio de dezenas: `MSB3274`. Um projeto **não pode referenciar outro de framework maior**, e o MSBuild, em vez de parar, **descarta a referência** e segue.
- **`CS1061`** — o painel chamava `InitializeInstagramWebScraping`, que a api não tem mais.
- **`MSB6006: lc.exe`** — o compilador de licenças da DevExpress, com **74 avisos `MSB3245`** escondendo que a versão instalada não era a 18.2.
- **WebCompiler** — uma extração interrompida em `%TEMP%`, e duas DLLs que não vêm do controle de versão.

E uma armadilha de configuração: o `ApifyToken` é lido por `ConfigurationManager.AppSettings` **dentro da DLL**, mas uma DLL lê a configuração **do processo que a carrega**. O token no serviço não serve ao painel. E o `Web.Release.config` usa `xdt:Transform="Replace"` no `<appSettings>`: uma chave que só existisse no `Web.config` sumiria no publish **sem aviso**.

**2. Compila, mas não carrega.** A api foi compilada contra **Newtonsoft 13**, **RestSharp 110** (que pede **System.Text.Json 7** e **Microsoft.Bcl.AsyncInterfaces 7**); o painel tinha Newtonsoft 12 e nem tinha System.Text.Json. Em .NET Framework, um assembly com **nome forte** exige a **versão exata** pedida — a DLL é encontrada e **recusada** (`FileLoadException 0x80131040`). O redirect antigo cobria `0.0.0.0-12.0.0.0` e o NuGet, ao atualizar o pacote, **não o atualizou**. Os avisos do Visual Studio citavam três conflitos e **não citavam** que duas dependências simplesmente não iam para o `bin\`.

Em produção, isso viraria a mensagem "Erro ao buscar dados" — porque o `RunSearch` engole qualquer exceção, sem log.

**3. Canaltech como um nó só.** Na árvore da programação, o UOL mostrava cada feed; o Canaltech era um nó único e a categoria só era escolhida num popup depois de arrastar. A DLL de produção **não podia mudar** — lendo o IL com `ildasm`, ela exige a chave exata `18_14` e lê a categoria de um combo:

```
IL_052f:  ldstr      "18_14"
IL_0534:  call       bool [mscorlib]System.String::op_Equality(string, string)
IL_0552:  ldfld      ... ProgramacaoCampanha::cbxCategoriaCanaltech
```

**4. Selecionar vários, arrastar, entrar um.** O `EndDragNode` mandava só `e.nodeKey`. O IL mostrou que o servidor **já aceitava várias chaves** (`Split(',')` — o `ldc.i4.s 44` é a vírgula) e o menu do botão direito já mandava várias. Faltava só o arrasto.

## A solução

### 1. Fazer o painel compilar (e o Instagram voltar)

| Onde | O que mudou |
|---|---|
| `PlayerVideo.csproj` | `TargetFrameworkVersion` de `v4.5.2` para **`v4.7.2`** — a correção do `CS0246` em massa foi **uma linha**. |
| `Web.config` | `<compilation targetFramework="4.7.2">` (a compilação dos `.aspx` feita pelo IIS). O `<httpRuntime>` ficou **sem** `targetFramework`, de propósito: ele liga outro modo de compatibilidade do ASP.NET, e a tarefa era compilar, não mudar comportamento. |
| `Web.config` **e** `Web.Release.config` | O `ApifyToken` nos dois, porque o transform troca o `<appSettings>` inteiro. |
| `FrmPluginSocialPlugin.aspx.cs` | `InitializeInstagramWebScraping` → **`InitializeInstagramSmartProxy`** (mesmos sete parâmetros, mesma ordem) e o campo `folderInstagramWebScraping` → `folderInstagramDownload`, porque o nome mentia sobre o fornecedor. |

### 2. Provar o carregamento sem subir o site

Pacotes subidos pelo NuGet (Newtonsoft **13.0.3**, NReco **1.2.0**, System.Text.Json **8.0.2** e dependências) e os `bindingRedirect` corrigidos à mão. A lição está na diferença entre **versão de pacote** e **versão de assembly** — é a segunda que vai no `Web.config`:

| Pacote | Versão do pacote | Versão do assembly |
|---|---|---|
| Newtonsoft.Json | 13.0.3 | 13.0.0.0 |
| System.Text.Json | 8.0.2 | 8.0.0.2 |
| System.Memory | 4.5.5 | 4.0.1.2 |
| System.Runtime.CompilerServices.Unsafe | 6.0.0 | 6.0.0.0 |

Para não depender dos avisos do Visual Studio, escrevi um **verificador em C#** (`Verificador.cs`, compilado com o `csc.exe` do próprio Windows) que pergunta ao próprio .NET:

- cria um **`AppDomain`** com `ApplicationBase` = uma cópia do `bin\` publicado e `ConfigurationFile` = o `<runtime>` do `Web.config` publicado — ou seja, **os mesmos redirects que o IIS aplicaria**;
- atravessa a fronteira com um objeto **`MarshalByRefObject`**;
- para cada DLL, lê o que ela pede com **`Assembly.ReflectionOnlyLoadFrom`** + `GetReferencedAssemblies()` e carrega de verdade com **`Assembly.Load(AssemblyName)`**;
- e **executa** os três trechos que o painel executa ao salvar um plugin, por reflexão: construir o `RestClient` do Apify, serializar com o System.Text.Json do RestSharp e gravar JSON pela `IOHelper` da api (Newtonsoft). A chamada real ao Apify ficou de fora de propósito: consumiria crédito e dependeria da rede.

### 3. Canaltech na árvore — só banco e JavaScript

- **Procedure `SPGetProgramacaoPluginFolder`**: um bloco `UNION ALL` gera um nó `18_14_<categoria>` para cada categoria **ativa** na tabela, com `ParentID = '18_14'`. O `ASPxTreeList` monta a hierarquia só por `ID`/`ParentID`, então o próprio nó do Canaltech vira o "grupo". Aplicada com **`ALTER`**, não `DROP`/`CREATE`, para manter as permissões.
- **`JSProgramacaoCampanha.js`**: lê a categoria do terceiro pedaço da chave, **traduz** `18_14_11` de volta para `18_14`, preenche o combo `cbxCategoriaCanaltech` e abre só o popup de tempo. O servidor recebe **exatamente** o que já recebia quando o usuário escolhia no popup antigo.
- **O bug antigo que o teste real achou.** Os sinalizadores `isYoutube`, `isCanaltech`, `isJovemPan`... são **globais** e só recebiam valor quando a chave começava com `18`. Arrastar um Website **depois** de um Canaltech abria o popup do Canaltech. O bug existia no JS original de produção; a nova funcionalidade só o tornou comum. Correção: **zerar todos** no início de `ParseKeyAndSetGlobals`.

Em 29/09 a **Jovem Pan** entrou no mesmo molde (bloco `18_18_*` e um ramo copiado no JS) — de propósito sem generalizar: com dois casos, a tabela de plugins seria abstração antecipada.

### 4. Arrastar vários de uma vez

- **`ChavesDoArrasto`** — se o item arrastado está marcado, leva todos os marcados (em ordem da árvore, sem pastas); se não, só ele. A API `GetVisibleSelectedNodeKeys` da DevExpress foi **lida dentro da DLL** antes de ser usada, para saber o limite real.
- **`ProcessAddVarios`** — um popup de tempo só e um único callback com as chaves separadas por vírgula. Se a seleção tiver Canaltech, YouTube, Futebol, Jovem Pan ou Campanha (que leem configuração de **um** combo por callback), **avisa e não adiciona nada** — melhor que entrar metade da seleção.
- **A terceira porta.** Depois dos testes, uma programação parou de abrir ("Runtime Error"). O log de eventos do servidor (evento ASP.NET 3005) mostrou `int.Parse(null)` em `cVideoProg.Nome`: o menu do botão direito, com 2+ itens, mandava as chaves cruas e gravava sem `Configuracao`. A mesma trava entrou no `NavBarMidias.ascx`, **perguntando à mesma função do JS** (`ChaveAbrePopupProprio`) em vez de repetir a lista. As duas linhas quebradas foram desativadas com um `UPDATE` **idempotente** (exclusão lógica, `AND Configuracao IS NULL`).

## Resultado

| | Antes | Depois |
|---|---|---|
| Build do painel | 4 camadas de erro (26× `CS0246`) | **0 erros** |
| Carregamento das dependências (cópia do `bin\` publicado) | Apify, System.Text.Json e Newtonsoft **falham** | **0 falhas de carga**, 3/3 execuções OK |
| `bindingRedirect` | Newtonsoft até 12 (pedido da 13 ficava de fora) | Newtonsoft 13, System.Text.Json 8.0.0.2, Bcl.AsyncInterfaces 8, Unsafe 6 |
| Canaltech na árvore | 1 nó + popup de categoria | **8 categorias** na árvore, só o popup de tempo |
| Arrastar com seleção | entrava 1 item | entram **todos**, com um popup de tempo |
| DLL do painel recompilada para os itens 3 e 4 | — | **nenhuma** (procedure + JS + markup do `.ascx`) |

### Como foi provado

- **Build e publish** conferidos por arquivo: `PlayerVideo.dll` com **4.211.200 bytes** contendo `InitializeInstagramSmartProxy` e não `InitializeInstagramWebScraping`; `Web.config` publicado com `ApifyToken`, `targetFramework="4.7.2"` e **sem** `debug` (prova de que o transform rodou); **49 DLLs DevExpress** e **123 satélites `pt-BR`** no `bin\`.
- **Verificador** em três cenários: publish como estava (três falhas), só Newtonsoft + NReco (duas falhas), tudo (**todas** as referências de todas as DLLs carregaram).
- **Harness em Node** (`vm`) com a tela simulada — árvore com seleção, combos, popups e `calPanel` que só registram chamadas — comparando o **log inteiro** do JS original e do novo, `.js` e `.min.js`: **12 → 21 → 29 cenários × 4 variantes, TUDO OK**. O cenário "Canaltech e depois Website" reproduz o bug antigo no original.
- **Procedure** rodada como consulta (só o `SELECT`, sem `ALTER`) para 4 usuários: mesmas linhas de antes **+ 8** `18_14_*`, **0 sumidas**.
- **Produção**: categorias na árvore, Website depois de Canaltech abrindo o popup certo, dois itens marcados entrando com o tempo digitado, aviso com dois Canaltech, e a programação quebrada voltando a abrir depois do `UPDATE`.

## Tecnologias e conceitos

- **ASP.NET Web Forms** · **.NET Framework 4.7.2** · C# · JavaScript (ES5) · **T-SQL** (SQL Server)
- **DevExpress 18.2** — `ASPxTreeList`, API cliente (`IsNodeSelected`, `GetVisibleSelectedNodeKeys`, `PerformCallback`)
- **MSBuild** — framework alvo, `ProjectReference`, `ResolveAssemblyReference`, `MSB3274`, `lc.exe`/`licenses.licx`
- **Transformação XDT** do `Web.config` (`Replace`, `RemoveAttributes`)
- **Nome forte**, `PublicKeyToken`, **`bindingRedirect`**, versão de pacote × versão de assembly
- **`AppDomain`**, **`MarshalByRefObject`**, **reflexão** (`ReflectionOnlyLoadFrom`, `Assembly.Load`, `Activator`, `TargetInvocationException`)
- **Leitura de IL com `ildasm`** para descobrir o contrato de uma DLL sem fonte
- **Estado global** e efeito colateral em JS; funções como valor (`some`, `filter`)
- **Árvore por `ID`/`ParentID`**, `UNION ALL`, `ALTER PROCEDURE`, exclusão lógica idempotente
- **Ordem de deploy produtor/consumidor** — quem lê o formato novo sobe antes de quem o cria

## Estrutura

```
AdmLegado/
├─ Pages/Plugin/FrmPluginSocialPlugin.aspx.cs   → a busca do Instagram de volta (InitializeInstagramSmartProxy)
├─ Config/
│  ├─ PlayerVideo.csproj.trecho.xml             → framework alvo 4.7.2, referências e ProjectReference da api
│  └─ Web.config.trecho.xml                     → <compilation>, <httpRuntime> e os bindingRedirect do <runtime>
├─ Verificador/Verificador.cs                   → AppDomain + reflexão: prova o carregamento sem subir o site
├─ SQL/SPGetProgramacaoPluginFolder-ALTER.sql   → nós 18_14_<categoria> na árvore (bloco UNION ALL do Canaltech)
├─ JS/JSProgramacaoCampanha.js                  → tradução da chave, sinalizadores zerados, arrasto de vários
└─ UserControl/NavBarMidias.ascx.trecho.js      → mesma trava no menu do botão direito
```

> 📄 **Os arquivos de configuração são trechos.** `Web.config.trecho.xml` traz só `<compilation>` (sem a lista de assemblies), `<httpRuntime>` e o `<runtime>`; o `.csproj` traz só o framework alvo e as referências citadas. O `<runtime>` é o da branch que vai para produção, que além dos redirects deste trabalho já tinha os de HtmlAgilityPack e System.Configuration.ConfigurationManager.
>
> 📄 **`Verificador.cs` foi reconstruído a partir do IL** do `Verificador.exe` usado nos testes (o `.cs` original ficou num diretório temporário). O comportamento é o mesmo, instrução por instrução, e o arquivo compila com o `csc.exe` do .NET Framework.
>
> 📄 O `JSProgramacaoCampanha.js` é a versão atual, já com a Jovem Pan (29/09). Em produção a página carrega o `.min.js`, que foi gerado por **trocas de texto exatas** sobre o `.min` original — cada trecho tinha de aparecer uma única vez — em vez de reminificar o arquivo inteiro e tornar a revisão impossível.

> 🔒 Nenhuma credencial neste diretório: `connectionStrings`, `appSettings` (incluindo o `ApifyToken`) e endereços internos ficaram de fora.
