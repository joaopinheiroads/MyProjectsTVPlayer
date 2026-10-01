# Projeto 10 — Integração Canva → TV Player: mídia direto na programação

Extensão do app do Canva do TV Player para que a arte criada no Canva **não pare no repositório de mídias**: com um clique, ela entra **direto na programação do terminal**, na posição escolhida pelo usuário. São **3 rotas novas** na `TVPlayerSite.API` (.NET Core 2.2), **espelhadas** na `CanvaAPI` (.NET 8), atravessando três processos — o app React dentro do Canva, a CanvaAPI intermediária e a API que fala com o banco.

> **A regra de "quais programações este usuário enxerga" não foi reescrita: foi reaproveitada.** Ela mora numa stored procedure com três ramos e ids de grupo fixos; a API chama essa procedure por **ADO.NET usando a mesma conexão do EF Core**, em vez de reimplementá-la em LINQ.
>
> *Junto veio um incidente de deploy: o publish da CanvaAPI saltou de **14 arquivos para 106 arquivos / 33,5 MB** — e voltou a 14 com seis linhas de `.csproj`.*

## O problema

O app do Canva já subia a mídia para o TV Player, mas o fluxo parava ali:

- **Meio caminho andado** — depois de publicar no Canva, o usuário ainda precisava abrir o painel administrativo e arrastar a mídia para dentro de uma programação.
- **Não havia por onde continuar** — a rota de upload devolvia nome de arquivo e MD5, mas **não o id da mídia** recém-criada. Sem esse número, nenhuma chamada seguinte conseguia dizer "põe *esta* mídia na programação".
- **Regra de acesso escondida no banco** — quem enxerga qual programação é decidido pela procedure `ap_GetProgramacaoByUsuarioID`, com três ramos e dois ids de grupo fixos. Copiar isso para C# daria certo no usuário de teste e erraria em produção.
- **Identidade vinda do cliente** — a rota de upload aceitava `userId` no formulário. Tolerável para gravar um arquivo; inaceitável para **alterar o que vai ao ar** num player.

## A solução

| Peça | Onde | Responsabilidade |
|------|------|------------------|
| `ProgramacaoResumo` | `TVPlayer.CRUD` | Objeto imutável de leitura para o retorno da procedure (não é tabela — `Categoria` é coluna calculada). |
| `IProgramacaoRepository` / `ProgramacaoRepository` | `TVPlayer.CRUD` | Executa `ap_GetProgramacaoByUsuarioID` por **ADO.NET** sobre a conexão do EF Core (`GetDbConnection()`), respeitando a transação corrente. |
| `IProgramacaoVideosRepository` / `ProgramacaoVideosRepository` | `TVPlayer.CRUD` | Lista ordenada da timeline (`Include` + `OrderBy(Ordem).ThenBy(Id)`) e a próxima ordem (`MAX + 1`). |
| `ProgramacaoController` | `TVPlayerSite.API` | `GET /api/programacao`, `GET /{id}/midias`, `POST /{id}/midia` — identidade **do claim do JWT**, três portões antes de escrever. |
| `InserirMidiaProgramacaoDTO` | `TVPlayerSite.API` | O que o cliente pode escolher: `videoId`, `duracao?`, `posicao?` — e nada mais. |
| `MidiaController` | `TVPlayerSite.API` | Uma linha a mais na resposta do upload: **`videoId`**, o elo entre as duas chamadas. |
| `ProgramacaoController` | `CanvaAPI` | Repasse fino das mesmas 3 rotas; `502 Bad Gateway` quando o TV Player recusa. |
| `ITVPlayerService` / `TVPlayerService` | `CanvaAPI` | Cliente HTTP tipado para a TVPlayerSite.API, repassando o `Bearer` do usuário. |
| Models (`ProgramacoesResponse`, `InserirMidiaRequest`...) | `CanvaAPI` | Espelho tipado das respostas da TVPlayerSite.API. |

### Os três portões antes do INSERT

1. **A programação está na lista da procedure?** — reaproveita a regra de grupo/empresa inteira, sem cópia.
2. **A mídia existe e está ativa?**
3. **A mídia é deste usuário?** — o `VideoID` é sequencial; sem esse teste, qualquer usuário autenticado poderia pôr a mídia de outro cliente na própria tela.

### Posicionar sem escrever SQL

Com `posicao` informada, a lista ordenada é carregada **rastreada**, o item novo entra com `List.Insert` e a timeline é **renumerada de 0 a n** — a mesma regra que o editor do painel aplica ao salvar. O EF Core gera os UPDATEs pelo change tracking e o INSERT pelo estado `Added`, **tudo num único `SaveChanges`** (uma transação). Sem `posicao`, a mídia vai para o fim (`MAX(Ordem) + 1`).

Duas regras foram **copiadas do painel** em vez de inventadas: imagem toca 15 s por padrão (faixa 1–14.400 s) e vídeo usa sempre a duração do próprio arquivo.

### O incidente do publish

O publish da CanvaAPI, que sempre teve **14 arquivos**, passou a sair com **106 arquivos e 33,5 MB**. Causa: o SDK `Microsoft.NET.Sdk.Web` inclui `**/*.json` e `**/*.config` como `Content` por **glob implícito**, e a pasta do projeto também abriga o front do Canva (`FRONT/`, com `node_modules`), mantido ali de propósito para ficar no mesmo versionador. Nenhum bug — a regra fez o que promete, sobre uma árvore que não foi pensada para ela.

A correção foi ensinar o build a ignorar, sem mexer na organização do repositório:

```xml
<ItemGroup>
  <Content Remove="FRONT\**" />
  <Content Remove="webconfigdocanva\**" />
  <Content Remove="alteracoes\**" />
  <None Remove="FRONT\**" />
  <None Remove="webconfigdocanva\**" />
  <None Remove="alteracoes\**" />
</ItemGroup>
```

Na investigação, mais uma lição: **data de arquivo no servidor prova a cópia, não a compilação**. O que confirmou qual binário estava no ar foi a lista de rotas do Swagger — e a leitura da DLL: a nova (74.240 bytes) contém `ProgramacaoController`; a de produção (70.144 bytes) não.

## Resultado

| | Antes | Depois |
|---|---|---|
| Da arte no Canva até a tela | upload + painel administrativo manual | **1 fluxo**, sem sair do Canva |
| Id da mídia na resposta do upload | não | **sim** (`videoId`) |
| Escolha da posição na timeline | só no painel | **no app** (`posicao`, base 1) |
| Regra de visibilidade das programações | — | **reaproveitada** da procedure (0 linhas duplicadas) |
| Identidade em escrita | `userId` no formulário | **claim `Id` do JWT** assinado |
| Publish da CanvaAPI | 106 arquivos / 33,5 MB (regressão) | **14 arquivos**, conjunto idêntico ao anterior |

### Como foi provado

- **Contra o banco real**, por um console que referencia só a `TVPlayer.CRUD`: a procedure devolveu a programação do usuário de teste, a próxima ordem saiu **42** (e não 41 — a coluna `Ordem` tinha um buraco, o que justificou `MaxAsync` em vez de `CountAsync`), e o INSERT completo rodou **dentro de uma transação com rollback** — a contagem voltou a 40.
- **O teste mais seguro achou o bug**: dentro da transação, o comando ADO.NET montado à mão falhou com `BeginExecuteReader requires the command to have a transaction...`. A correção é uma linha — `command.Transaction = _context.Database.CurrentTransaction?.GetDbTransaction();` — e evita a falha no dia em que alguém envolver a inserção numa transação.
- **Build** com 0 erros nas duas soluções (`net8.0` e `netcoreapp2.2`); testes Jest do front conferindo o corpo `{ videoId, posicao, duracao }`.
- **Publish** regerado e comparado arquivo a arquivo com o anterior: 14 arquivos, nenhuma subpasta.

### Cuidados de contrato

- **Publique primeiro quem recebe o campo novo.** A API 2.2 usa Newtonsoft.Json, que **ignora em silêncio** propriedade desconhecida: CanvaAPI nova com API velha poria toda mídia no fim, sem erro.
- **Texto montado no backend não vai para tela traduzida.** A listagem devolve dado (nome da mídia, tipo do item); plugins voltam com `nome = null` e o app mostra o rótulo traduzido — exigência da revisão do Canva.

## Tecnologias e conceitos

- **ASP.NET Core** em duas gerações: .NET Core 2.2 (TVPlayerSite.API) e **.NET 8** (CanvaAPI) · C#
- **EF Core 2.2** — `Include`, `OrderBy`/`ThenBy`, change tracking, `SaveChanges` atômico
- **ADO.NET** sobre a conexão do EF Core — `GetDbConnection()`, `CommandType.StoredProcedure`, `DbParameter`, `CurrentTransaction?.GetDbTransaction()`
- **SQL Server** — stored procedure como fonte única da regra de acesso
- **JWT** com `[Authorize]` e identidade por **claim**
- **DTO** como fronteira de entrada (só entra o que está declarado)
- **Repository + Unit of Work**, **Injeção de Dependência**, **`IHttpClientFactory`** com cliente tipado
- **System.Text.Json** × **Newtonsoft.Json** entre as duas APIs
- **MSBuild** — projeto SDK-style, globs implícitos, `Include` / `Remove` / `Update`

## Estrutura

```
CanvaProgramacao/
├─ CanvaAPI/                                   → .NET 8, intermediária entre o app do Canva e o TV Player
│  ├─ CanvaAPI.csproj                          → com os Remove que devolveram o publish a 14 arquivos
│  ├─ Program.cs                               → DI, HttpClient tipado, CORS do Canva
│  ├─ Controllers/
│  │  ├─ ProgramacaoController.cs              → repasse das 3 rotas (502 quando o TV Player recusa)
│  │  └─ UploadController.cs                   → upload que agora devolve o videoId do TV Player
│  ├─ Services/
│  │  ├─ ITVPlayerService.cs                   → contrato
│  │  └─ TVPlayerService.cs                    → chamadas HTTP com o Bearer do usuário
│  └─ Models/                                  → espelho tipado das respostas (Programacoes, Midias, InserirMidia...)
└─ TVPlayerSite/
   ├─ TVPlayer.CRUD/                           → .NET Core 2.2, acesso a dados
   │  ├─ Classes/ProgramacaoResumo.cs          → retorno da procedure (somente leitura)
   │  ├─ Interfaces/Repositories/
   │  │  ├─ IProgramacaoRepository.cs
   │  │  └─ IProgramacaoVideosRepository.cs
   │  └─ Repositories/
   │     ├─ ProgramacaoRepository.cs           → stored procedure via ADO.NET na conexão do EF Core
   │     └─ ProgramacaoVideosRepository.cs     → timeline ordenada + próxima ordem
   └─ TVPlayerSite.API/
      ├─ Controllers/
      │  ├─ ProgramacaoController.cs           → listar, listar mídias, inserir na posição
      │  └─ MidiaController.cs                 → upload (passa a devolver videoId)
      ├─ DTO/InserirMidiaProgramacaoDTO.cs     → videoId, duracao?, posicao?
      └─ Interfaces/UnitOfWork/IVideoUnitOfWork.cs
```

> 📄 Recorte do código real: só os arquivos tocados por esta entrega. O front do Canva (React/TypeScript), as entidades geradas por scaffold e o `VideoContext` ficaram de fora.

> 🔒 Nenhuma credencial neste diretório: URLs da API, token de serviço e conexões vêm de configuração (`appsettings.json`, não versionado aqui); hosts internos foram substituídos por `<REDACTED>`.
