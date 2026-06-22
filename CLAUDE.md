# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Purpose of this repo

A **portfolio / showcase repo** collecting work the author did at TVPlayer/MapMaker, meant for recruiters and other devs to browse. Goal is to keep it **simple and objective**. **Do not commit sensitive information** (API tokens, DB connection strings, account keys) — see the security warning under Project 2. (See the `<!-- SPECKIT -->` block at the bottom for the author's original brief, in Portuguese.)

## Repository shape

Two **unrelated** projects live under one Git root — no shared code, language, or build. `Readme.md` (Portuguese) is the portfolio landing page.

- **`Project 1/`** — static weather widgets for digital signage / indoor TV media (no backend, no build).
- **`Project 2/`** — `Cardapio`, a digital-menu REST API backend (.NET 5, ASP.NET Core).

Each project directory also contains its **own nested `.svn/` working copy** (TortoiseSVN). SVN and Git coexist here — a change inside `Project 1/` or `Project 2/` is tracked by both. Be aware when committing.

---

## Project 1 — Weather widgets (`Project 1/`)

Pure HTML/CSS/vanilla-JS screens designed to be loaded full-screen on TV/signage players. **No build, no package manager, no server code.** All data comes from the **URL query string** — nothing is fetched at runtime.

Layout: two visual designs (`MODELO 2/`, `MODELO 3/`), each with a `1dia/` (single-day) and `4dias/` (4-day forecast) variant. JS entry points: `clima_1d.js` (1-day) and `script4d.js` (4-day).

How it works:
- `parseQuery()` reads `window.location.search` into an `options` object; every field (`cidade`, `max`, `min`, `clima`, `idioma`, etc.) is a query param.
- `yCode` is a **Yahoo weather condition code**; it drives both the weather icon (`wi wi-yahoo-<code>` from `weather-icons.min.css`) and the chosen background image in the `backgroundByClimaCode*` switch functions.
- `modelo` (1/2/3) selects rendering mode; orientation + viewport size pick HD vs Full-HD and portrait vs landscape background assets from `assets/`.
- `idioma` (`ptbr`/`eng`) and `unidadeTemp` (`c`/`f`) toggle on-screen text via the `ChangeText*` helpers.

To preview: serve the folder with any static server and open the HTML with a full query string. **`MODELO 2/QueryString.txt` holds working example URLs** for both `1dia` and `4dias` (sample uses Live Server on port 5500).

---

## Project 2 — Cardapio API (`Project 2/`)

ASP.NET Core (`net5.0`) solution `Cardapio.sln` with two projects:

- **`Cardapio.CRUD/`** — data/domain layer: EF Core `Models`, `CardapioContext`, `Migrations`, `Repositories`, `UnitsOfWork`, `DTOs`, AutoMapper `Profiles`, `CustomValidation`. It has its own `Program`/`Startup`, but that host is a leftover stub (`Configure` only returns "Hello World!") — **do not run `Cardapio.CRUD` as the app.**
- **`Cardapio.API/`** — the real host: `Controllers`, JWT auth, Azure Blob image storage. References `Cardapio.CRUD`.

### Architecture (read these patterns before editing controllers/repos)

- **Unit of Work + Repository.** Controllers depend only on `ICardapioUnitOfWork`. `CardapioUnitOfWork` lazily creates each repo (`??=`), all sharing one `CardapioContext`. Repos **never** save — the controller calls `cardapioUnitOfWork.Commit()` (which is `SaveChangesAsync`) to persist a whole operation in one transaction. Quirk: `ProdutoRepo.UpdateAsync` is a no-op that relies on EF change tracking; the actual write happens in `Commit()`.
- **Multi-tenancy by `EmpresaID`.** Every domain entity belongs to an `Empresa`. Controllers extract `EmpresaID` and `UsuarioID` from JWT claims (`Helpers/CustomClaimTypes.cs`) and pass `empresaID` into repo queries — this is the tenant scoping, so don't drop it. Endpoints return `NotFound()` when the claim is missing/unparsable.
- **Soft deletes + audit + conventions.** `OnModelCreating` applies model-wide conventions: any `Ativo` column defaults to `true`, any `DataCadastro` defaults to SQL `getdate()`, and **all** FK delete behavior is `Restrict`. "Delete" means setting `Ativo = false`, not removing rows. Entities carry `UsuarioIDCadastro`/`UsuarioIDEdicao` audit fields, set from the JWT user id.
- **Auth.** JWT bearer (HS256) + DB-persisted refresh tokens (`RefreshToken` entity, `UsuarioService`). Access-token lifespan 2h; refresh token 14 days. `Startup` sets `ValidateIssuer`/`ValidateAudience = false`.
- **Images.** `ProdutoController` + `FileManagerHelper` upload product images and generated thumbnails to **Azure Blob Storage** (Produto/Thumbnail containers), reading width/height/size from blob metadata. CRUD on `Produto` cascades to its images/thumbnail.
- **Mapping/serialization.** AutoMapper with collection mappers (`AddCollectionMappers`) maps Entity ↔ DTO; profiles live in `Cardapio.CRUD/Profiles`. JSON uses Newtonsoft with camelCase. `CardapioContext` is registered with **`Transient`** lifetime in the API (intentional — keep it unless you understand the UoW implications).

### Commands (run from repo root)

```bash
# Build the solution
dotnet build "Project 2/Cardapio.sln"

# Run the API (https://localhost:5001 ; http://localhost:5000)
dotnet run --project "Project 2/Cardapio.API"

# EF Core migrations: model lives in Cardapio.CRUD, host config in Cardapio.API
dotnet ef migrations add <Name> --project "Project 2/Cardapio.CRUD" --startup-project "Project 2/Cardapio.API"
dotnet ef database update        --project "Project 2/Cardapio.CRUD" --startup-project "Project 2/Cardapio.API"
```

There is **no test project** checked in (the README mentions xUnit/Moq, but none exists). The connection-string key is `MenuConnectionString`; the blob-storage key is `AzureBlobStorage`.

> ⚠️ **Secret leak.** `Cardapio.API/appsettings.json` currently has **live Azure SQL credentials, a Blob Storage account key, and the JWT secret committed in plaintext.** This directly violates the repo's "no sensitive info" goal. Before this repo is pushed publicly, scrub these (use user-secrets/env vars), and treat the leaked values as compromised (rotate them). Do not add new secrets to tracked files.

---

## Spec Kit & caveman

This repo is initialized with [Spec Kit](.specify/) — the `speckit-*` skills and `.specify/` templates/workflows drive a spec → plan → tasks → implement flow. When a feature is developed through Spec Kit, the active plan under `.specify/`/`specs/` is the source of truth for that feature's tech choices and steps. The author also prefers the **caveman** skill to reduce token usage during this work.

<!-- SPECKIT START -->
Você vai me ajudar a fazer uma repositório no github com o propósito de armazenar alguns projetos que realizei durante meu padrão na TVPlayer/MapMaker.
Faremos algo simples e objetivo para que possamos comprovar meus trabalhos, não vamos subir informações sensíveis como tokens e acesso ao banco de Dados.
O repositório servirá para que recrutadores e outros dev possam ver trabalhos que foram realizados, pesquise no mercado maneiras eficaz de realizar o procedimento. Para concluir o serviço
com eficácia e eficiência você utilizará caveman plugin para economizar Tokens e Spec Kit
<!-- SPECKIT END -->
