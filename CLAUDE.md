# CLAUDE.md

Este arquivo orienta o Claude Code (claude.ai/code) ao trabalhar com o código deste repositório.

## Propósito do repo

Um **repositório de portfólio** reunindo trabalhos que o autor realizou na TVPlayer/MapMaker, para recrutadores e outros devs navegarem. A meta é manter tudo **simples e objetivo**. O `Readme.md` (em português) é a página de entrada; cada `Project N/` tem seu próprio `Readme.md` com o detalhe técnico.

**O repositório é público** — `https://github.com/joaopinheiroads/MyProjectsTVPlayer`, branch `main`. Todos os Readmes afirmam que o repo não contém credenciais. **Leia a seção de segurança abaixo antes de confiar nessa afirmação.**

(O bloco `<!-- SPECKIT -->` no final guarda o briefing original do autor.)

---

## 🚨 Segurança — leia antes de commitar qualquer coisa

### Segredo real publicado neste momento

`Project 3/CardapioDigitalSite/EscolhaAI/Services/EmailService.cs` (~linha 36) tem uma **senha SMTP real, sem redação**, da conta `escolha@escolha.ai`, hardcoded no C#, com um comentário ao lado identificando-a como a senha. **Esse arquivo está rastreado e já foi enviado para o GitHub público.** O `.gitignore` não o cobre (ele ignora `appsettings.json`, não arquivos `.cs`).

Essa credencial precisa ser **rotacionada** — ela é pública. Limpar o arquivo não basta: a senha permanece no histórico do Git e exige reescrita de histórico (`git filter-repo`) mais um force-push. A causa raiz é que o host `EscolhaAI` **não tem nenhum `appsettings.json` no servidor**, então a senha acabou embutida no código.

### Segredos no disco, mas NÃO publicados (não commite)

O `.gitignore` cobre `appsettings.json`, `appsettings.*.json`, `*.pfx`, `*.key`, `*.pem`, `.env`, `[Bb]in/`, `[Oo]bj/`, `dist/`, `.svn/`. Verificado: o `git ls-files` rastreia **apenas** o `Project 5/IntegracaoChatIA/chatia/appsettings.example.json`. Os arquivos abaixo existem localmente, estão corretamente ignorados e **nunca devem ser adicionados à força**:

| Arquivo | Contém |
|---|---|
| `Project 2/CardapioDigital/Cardápio/Cardápio/appsettings.json` | Connection string real do SQL Server (senha em texto puro) + chave de assinatura JWT real |
| `Project 5/IntegracaoChatIA/chatia/appsettings.json` | Chave real da API da Anthropic (`sk-ant-api03-…`), senha SQL real, `ADMIN_TOKEN_SECRET` real |
| `Project 5/…/chatia/bin/`, `obj/` e uma cadeia aninhada `publish/publish/publish/` | ~8 cópias adicionais, em texto puro, dos mesmos três segredos |
| `Project 6/Impostometro/cert.pfx` | Chave privada TLS real (PKCS#12) |

**Atenção:** vários desses segredos estão commitados nos **working copies aninhados do SVN** (ex.: `Project 5/IntegracaoChatIA/.svn/pristine/`). O Git ignorá-los não os remove do histórico do SVN. Se esses repositórios SVN forem acessíveis por outras pessoas, trate a chave da Anthropic, as senhas SQL, o `ADMIN_TOKEN_SECRET` e o certificado como comprometidos e rotacione-os.

### A convenção de redação

A maioria dos projetos passou por uma limpeza manual que substitui segredos em **arquivos de código** por marcadores entre colchetes — `[SENHA_SMTP_REMOVIDA]`, `[TOKEN_CHATPRO_REMOVIDO]`, `[INSTANCIA_REMOVIDA]`, `[PASSPHRASE_REMOVIDA]`. **Preserve esses marcadores**; nunca restaure um valor real. A limpeza cobriu arquivos `.cs`/`.ts`, mas **passou batido pelos `appsettings.json` e pelo `EmailService.cs` do Project 3** — essa falha é exatamente o vazamento acima.

Valores públicos por natureza são aceitáveis e **não** são vazamentos: a **site key** do reCAPTCHA em `Project 7/…/Index.cshtml`, o token do Google Search Console em `MyWebSite/public/googlec495f30b9114ae7f.html` e o **client ID** do Google OAuth no Project 2. Dados reais mas não secretos que mesmo assim estão publicados: o usuário/host SMTP `site@tvplayer.com.br` / `smtp.tvplayer.com.br`, o telefone comercial em `Project 8/…/DisparosChat.cs` e IPs internos (`191.6.5.106`) nos Projects 5 e 6.

---

## Formato do repositório

A raiz do Git é `MyProjects/`. Ela contém **nove pastas de projetos não relacionados** mais o `Readme.md` de entrada. Não há código, linguagem ou build compartilhados entre eles — não procure por uma solution comum ou um build na raiz.

**Apenas `Project 1`–`Project 8`, `Readme.md`, `CLAUDE.md` e `.gitignore` estão rastreados e publicados.** Os itens abaixo estão untracked (existem localmente, ausentes do GitHub):

- **`Project 9/`** — o projeto mais recente; ainda não commitado.
- **`MyWebSite/`** — o site de portfólio em React/Vite. Ironicamente, o site publicado direciona visitantes para este repo, mas o código do próprio site nunca foi enviado para cá.
- **`MyProjects/`** (aninhado, mesmo nome da pasta pai) — veja Spec Kit abaixo.

### A maioria dos projetos são extracts, não aplicações executáveis

**Isto é o mais importante a saber antes de editar.** Os Projects **4, 7, 8 e 9** são *extracts* de código recortados da solution bem maior `TVPlayerSite`: **não têm `.csproj` nem `.sln`** e referenciam tipos que não foram extraídos (`VideoContext`, `TabCliente`, `BaseCadastroController`, `APIClient.Factory`, `TVPlayer.CRUD.Models`). **Não compilam de forma isolada, e isso é intencional** — são material de referência para leitura. Não tente "consertar" as referências ausentes nem criar um arquivo de projeto. O target framework deles não é determinável a partir dos arquivos presentes.

Somente os Projects **1, 2, 3, 5, 6** e o `MyWebSite` são autocontidos o bastante para buildar/rodar.

### SVN e Git coexistem

Várias pastas carregam seu **próprio working copy aninhado do SVN** (TortoiseSVN): `Project 1/.svn`, `Project 2/.svn`, `Project 3/CardapioDigitalSite/.svn` (um nível mais fundo), `Project 5/IntegracaoChatIA/.svn`, `Project 6/Impostometro/.svn`. Uma alteração dentro dessas pastas é rastreada pelos **dois** sistemas. O Git ignora `.svn/`, então um commit no SVN pode publicar algo que o Git aparentemente excluiu — foi assim que os segredos acima chegaram ao histórico do SVN.

---

## Os projetos

| # | Pasta | O que é | Stack | Builda? |
|---|---|---|---|---|
| 1 | `Project 1/PluginPrevisaoDoTempo/` | Telas de previsão do tempo para sinalização digital | HTML/CSS/JS puro | Só servidor estático |
| 2 | `Project 2/CardapioDigital/` | Plataforma de cardápio digital (back-office + cardápio QR) | Blazor Server, .NET 8, EF Core, SignalR | ✅ |
| 3 | `Project 3/CardapioDigitalSite/` | Site institucional EscolhaAÍ | Blazor WASM + host Server, .NET 8, MailKit | ✅ (sem `.sln`) |
| 4 | `Project 4/IntegracaoKommo/` | Sincronização do funil do Kommo CRM (webhook in, PATCH out) | ASP.NET Core Web API, EF Core | ❌ extract |
| 5 | `Project 5/IntegracaoChatIA/chatia/` | Widget de chat de suporte com Claude e streaming SSE | .NET 8, Anthropic SDK 12.29.1, SQL Server | ✅ |
| 6 | `Project 6/Impostometro/` | Scraper do impostômetro + API para sinalização | Node, TypeScript, Express 5, Puppeteer 24 | ⚠️ veja abaixo |
| 7 | `Project 7/CodigoVerificacao/` | Código de verificação do cadastro de demonstração (WhatsApp + e-mail) | ASP.NET Core MVC, jQuery | ❌ extract |
| 8 | `Project 8/VerificadorDemonstracao/` | Serviços de background que alertam sobre demonstrações a vencer | `IHostedService`, EF Core/ADO.NET | ❌ extract |
| 9 | `Project 9/RefatoracaoDemonstracao/` | Antes/depois da refatoração do controller do Project 7 (825 → 199 linhas) | ASP.NET Core MVC, SOLID, DI | ❌ extract (proposital) |

### Comandos que funcionam

```bash
# Project 2 — atenção ao caminho ACENTUADO; sempre entre aspas
dotnet build "Project 2/CardapioDigital/Cardápio.sln"
dotnet run --project "Project 2/CardapioDigital/Cardápio/Cardápio/Cardápio.csproj"   # Swagger em /api-docs (só em Dev)

# Project 3 — não existe .sln; builde o host (ele referencia o client)
dotnet run --project "Project 3/CardapioDigitalSite/EscolhaAI/EscolhaAI.csproj"

# Project 5 — https://localhost:51867 ; publica no IIS sob o path base /chatia
dotnet run --project "Project 5/IntegracaoChatIA/chatia/ChatIA.csproj"

# MyWebSite (untracked) — React/Vite, publicado na Vercel
cd MyWebSite && npm install && npm run dev    # também: build / preview / lint
```

**O Project 6 não roda como documentado.** O `npm start` aponta para `dist/server.js`, mas o `tsconfig.json` está com o `outDir` comentado, então o `tsc` emite **no próprio lugar** — não existe `dist/`. O entry real é `node scraper.js`. O `npm run build` provavelmente também falha: o `tsconfig` define `"types": []` sem `@types/node`, mas o `scraper.ts` importa `https` e `fs`. E o `cert.pfx` não pode ser aberto porque a passphrase está redigida como `[PASSPHRASE_REMOVIDA]`. Os arquivos compilados `scraper.js`/`.d.ts`/`style.css` estão commitados ao lado dos fontes.

---

## Armadilhas por projeto

Leia primeiro o `Readme.md` do projeto, mas estas são as pegadinhas que os Readmes não contam:

- **Project 2** — `Cardápio.Client` é uma **class library** Razor, não WASM. O `AppDbContext` é `Scoped`; o JWT valida issuer/audience com tokens de 7 dias. O `AuthService` assina com `Encoding.ASCII` enquanto o `Program.cs` valida com `UTF8` — só funciona porque a chave é ASCII. O `EnableSensitiveDataLogging()` está ligado incondicionalmente (inclusive em Produção), o CORS é `AllowAnyOrigin`, a validação TLS está desabilitada no `HttpClient` injetado e as chaves de DataProtection vão para um `C:\keys` hardcoded. Arquivos soltos na raiz de `CardapioDigital/` (`PromocaoHorarioController.cs`, scripts `.sql`, `teste_carrinho_*.html`) estão fora dos dois csproj — código morto.
- **Project 4** — o webhook é **form-urlencoded lido por indexação de chave em string** (`contacts[add][0][custom_fields][0]…`), então **a ordem dos campos na configuração do Kommo é crítica**: reordenar troca telefone por e-mail silenciosamente. Os IDs das etapas do funil são consts hardcoded. Tanto `ReceiveWebHook` quanto `mover-lead` são `[AllowAnonymous]`, sem verificação de assinatura.
- **Project 5** — os frames SSE são escritos **à mão** no `Response.Body`; um erro depois que os headers já foram enviados vira um evento SSE `error`, não um status HTTP. O prompt caching coloca o breakpoint no *segundo* bloco de system, então editar a Persona invalida o cache. Todos os serviços são **Singletons**. A `JsonNamingPolicy` é deliberadamente `null` (PascalCase) para preservar o contrato antigo do Node — o `admin.html` depende disso; não "conserte". O CORS é totalmente aberto num `POST /api/chat` anônimo e sem rate limiting. Se `ADMIN_USER_IDS` estiver vazio, a checagem da allowlist de admin é **ignorada**.
- **Project 6** — o handler HTTP é **pura leitura de cache**; o scraping acontece num `setInterval` de 5 min. O servidor começa a responder `{ valor: "0" }` antes do primeiro scrape chegar. O scrape depende do seletor `#counterBrasil .counter-inside`; se o markup do site alvo mudar, os erros são engolidos e o valor velho é servido para sempre.
- **Project 7** — o reCAPTCHA **é** validado no servidor, mas no `BaseCadastroController` (que não foi extraído aqui), via uma action `Verify` separada; o POST da demonstração em si só verifica se o token não está vazio. Por isso o `CaptchaVerification.cs` extraído parece código morto, mas não é. O fluxo também envia a **senha em texto puro** do usuário por e-mail/WhatsApp, o que implica armazenamento reversível na origem.
- **Project 8** — o Readme diz "executa 1x por dia (às 10h)", mas os timers **fazem polling** (a cada 10–20 min) e cada tick checa `DateTime.Now.Hour == 10` protegido por um `static bool` não thread-safe. **O `#if DEBUG` define a hora de disparo como `25`** — uma hora que nunca ocorre — então os dois jobs ficam **permanentemente inertes em builds Debug**. Os callbacks do timer são `async void`. Comentários dizendo "8h" estão desatualizados; a constante é `10`.
- **Project 9** — mantém propositalmente duas cópias do mesmo controller (`Antes/` com 825 linhas, `Depois/` com 199). Não "desduplique"; o diff **é** a entrega. O `Depois/Startup.DI.cs` é um recorte ilustrativo, escrito à mão, do `ConfigureServices` real — não é um arquivo do código-fonte.

---

## MyWebSite (untracked)

Portfólio single-page em React 18 + Vite 5 + TypeScript 5.5 + Tailwind 3.4, publicado na **Vercel** (`https://vite-react-zeta-ashen.vercel.app/`). Entry `index.html` → `src/main.tsx` → `src/App.tsx` (a página inteira; o conteúdo fica em arrays const no nível do módulo). **Não há configuração de CI/CD** — sem `vercel.json`, sem `.github/`. O `vite.config.ts` não define `base`, então o build assume hospedagem na raiz (compatível com a Vercel, quebraria num subpath do GitHub Pages).

**O site e o repo divergiram.** O `src/App.tsx` lista **4** cards de projeto contra as **9** pastas do repo: dois apontam para a raiz do repositório (não para nenhum `Project N/`) e dois para sites externos. Os Projects 3–9 não estão representados. A pasta `Certificados/` e os `.jpg` soltos na raiz de `MyWebSite/` estão **fora de `public/`** e não são servidos.

## Spec Kit — instalado no lugar errado

O briefing/Readme da raiz diz que o repo usa [Spec Kit](https://github.com/github/spec-kit), mas **não existe `.specify/` nem `.claude/` na raiz.** Eles vivem no `MyProjects/MyProjects/` aninhado, que não contém *mais nada* — sem código, sem `specs/`.

Foi um acidente: o `.specify/init-options.json` registra `"here": false`, ou seja, o `specify init MyProjects` foi rodado sem `--here` e criou um subdiretório com o nome do projeto. Consequência: as dez skills `speckit-*` estão **escopadas nesse subdiretório vazio**, então só ativam para arquivos sob `MyProjects/MyProjects/` — onde não há código. O Spec Kit está instalado, porém efetivamente não funcional, e seu scaffolding está intocado (o `constitution.md` ainda tem placeholders `[PROJECT_NAME]`).

Para funcionar, `.specify/` e `.claude/` precisariam subir um nível. Até lá, não conte com as skills `speckit-*` a partir da raiz. O autor também prefere a skill **caveman** para economizar tokens.

---

## Convenções ao adicionar um projeto

Siga o padrão estabelecido — os quatro últimos projetos seguem todos ele:

1. Crie `Project N/<NomeDoModulo>/` com o código e um `Project N/Readme.md` em **português**.
2. O Readme abre com um resumo de um parágrafo, depois cobre o fluxo, a stack, a árvore de estrutura e uma nota de segurança. Se o módulo for um extract, **diga isso explicitamente** e cite o projeto de origem (veja os Projects 7/8/9).
3. Adicione uma linha na tabela do `Readme.md` da raiz — as colunas são `# | Projeto | Resumo | Stack | Pasta | Status`, com `✅` para documentado.
4. **Limpe os segredos antes de copiar**, usando a convenção de marcador `[SEGREDO_REMOVIDO]`, e passe um grep no resultado para confirmar.

<!-- SPECKIT START -->
Você vai me ajudar a fazer uma repositório no github com o propósito de armazenar alguns projetos que realizei durante meu padrão na TVPlayer/MapMaker.
Faremos algo simples e objetivo para que possamos comprovar meus trabalhos, não vamos subir informações sensíveis como tokens e acesso ao banco de Dados.
O repositório servirá para que recrutadores e outros dev possam ver trabalhos que foram realizados, pesquise no mercado maneiras eficaz de realizar o procedimento. Para concluir o serviço
com eficácia e eficiência você utilizará caveman plugin para economizar Tokens e Spec Kit
<!-- SPECKIT END -->
