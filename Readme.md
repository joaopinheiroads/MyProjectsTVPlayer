# 💻 Portfólio - Dev Full Stack C#

Bem-vindo! Sou Dev Full Stack especializado em **C# / .NET / ASP.NET Core** (e front-end com Blazor, JavaScript e React).

Este repositório reúne projetos reais que desenvolvi durante minha atuação na **TVPlayer / MapMaker**, com o objetivo de comprovar meus trabalhos. Cada projeto tem um **README próprio** com mais detalhes; abaixo está apenas um resumo de cada um.

> 🔒 Este repositório é público e **não contém credenciais, tokens ou acessos a banco de dados**.

---

## 👨‍💻 Sobre Mim

Dev Full Stack com foco em qualidade, performance e código limpo, com experiência desenvolvendo sistemas em produção.

### Principais responsabilidades
- Ciclo completo de desenvolvimento: da análise de requisitos ao deploy em produção.
- Design e implementação de arquiteturas escaláveis (Repository Pattern, Dependency Injection, Unit of Work, Clean Architecture).
- Desenvolvimento de **APIs REST** em ASP.NET Core para integração com múltiplos sistemas.
- **Integrações estratégicas**: CRMs (Kommo, DigiSac), APIs de IA (Claude/Anthropic, OpenAI), automação (n8n) e sistemas legados.
- Condução de reuniões técnicas, levantamento de requisitos e definição de roadmap técnico.

---

## 🛠 Tecnologias

**Backend:** C# · .NET / .NET Core · ASP.NET Core (MVC e Web API) · Blazor · Entity Framework Core · JWT · SQL Server
**Front-end:** Blazor · JavaScript (ES6+) · React / TypeScript · HTML5 · CSS3 · Razor
**IA & Integrações:** Claude (Anthropic SDK) · OpenAI · Kommo CRM · n8n
**DevOps & Ferramentas:** Git/GitHub · SVN (TortoiseSVN) · Azure (App Service, Blob Storage) · Docker · IIS · Swagger/Postman · Visual Studio / VS Code

---

## 🚀 Projetos

> Cada projeto tem um README próprio na sua pasta com os detalhes técnicos. A tabela abaixo é só o resumo.

| # | Projeto | Resumo | Stack | Pasta | Status |
|---|---------|--------|-------|-------|--------|
| 1 | **Plugin de Previsão do Tempo** | Plugin de previsão do tempo para sinalização digital, com troca de layout e idioma via query string e layout responsivo (4K/FHD/HD/LED, horizontal e vertical). | HTML · CSS · JavaScript | `Project 1` | ✅ |
| 2 | **Cardápio Digital — Plataforma** | Plataforma de gerenciamento do cardápio digital (produtos, categorias, pedidos e administração) + cardápio do cliente por QR Code, com pedidos em tempo real. | Blazor (.NET 8) · EF Core · SignalR | `Project 2` | ✅ |
| 3 | **Cardápio Digital — Site Institucional** | Site institucional (EscolhaAÍ) de apresentação da plataforma, com formulários de contato/newsletter por e-mail. | Blazor WebAssembly (.NET 8) · MailKit | `Project 3` | ✅ |
| 4 | **Integração Kommo CRM** | Integração do sistema TVPlayer com o Kommo CRM: recebe webhooks e move leads automaticamente no funil de vendas (via API v4 do Kommo). | ASP.NET Core Web API · EF Core | `Project 4` | ✅ |
| 5 | **Chat com IA (Claude)** | Assistente de suporte interno com IA da Anthropic (Claude) embutido no painel como widget, com streaming (SSE), prompt caching e persona anti-jailbreak. | .NET 8 · Anthropic SDK · SSE · SQL Server | `Project 5` | ✅ |
| 6 | **Plugin Impostômetro** | Exibe o valor do Impostômetro em tempo real para sinalização digital: scraping com Puppeteer + API própria atualizada a cada 5 min. | Node · TypeScript · Express · Puppeteer | `Project 6` | ✅ |
| 7 | **Código de Verificação (cadastro de demonstração)** | Tela e back-end do código de verificação no formulário de demonstração (código de 4 dígitos por WhatsApp/e-mail, expira em 5 min) — front e back desenvolvidos por mim. *(Faz parte do Projeto 4.)* | ASP.NET Core MVC · jQuery/AJAX · reCAPTCHA · SMTP | `Project 7` | ✅ |
| 8 | **Verificador de Demonstrações** | Serviços de background que detectam demonstrações a vencer/vencidas e disparam WhatsApp de alerta/contratação (e re-engajam quem não confirmou o e-mail). *(Faz parte do Projeto 4.)* | .NET · IHostedService · EF Core/ADO.NET · WhatsApp API | `Project 8` | ✅ |
| 9 | **Refatoração do fluxo de Demonstração** | Controller de 825 linhas que fazia tudo (código, cadastro, licença, e-mail, WhatsApp e log) refatorado em 7 passos para 199 linhas + 8 serviços injetados por interface — mesmo comportamento, sem mudar contrato. *(Refatora o Projeto 7.)* | ASP.NET Core MVC · SOLID (SRP/DIP) · Injeção de Dependência · Options Pattern | `Project 9` | ✅ |
| 10 | **Integração Canva → TV Player: mídia direto na programação** | O app do Canva passa a inserir a arte criada direto na programação do terminal, na posição escolhida, reaproveitando a regra de acesso de uma stored procedure via ADO.NET na conexão do EF Core. Inclui a correção de um publish que inchou de 14 para 106 arquivos por globs implícitos do MSBuild. | ASP.NET Core 2.2 + .NET 8 · EF Core · ADO.NET · SQL Server · JWT · MSBuild | `Project 10` | ✅ |
| 11 | **Disparos de WhatsApp confiáveis** | Corrige o incidente em que clientes receberam o mesmo WhatsApp 3x na mesma manhã (a trava saiu de um campo estático para uma coluna no banco, com SQL parametrizado e falha fechada) e acrescenta o template DigiSac com imagem no cabeçalho, com testes. *(Continua o Projeto 8.)* | C# · ASP.NET Core · IHostedService · ADO.NET/SQL Server · DigiSac API · xUnit | `Project 11` | ✅ |
| 12 | **Manutenção do painel legado (Web Forms)** | Painel ASP.NET Web Forms em produção: voltar a compilar (framework alvo, transformação de Web.config), provar o carregamento das DLLs com bindingRedirect e um verificador em C# (AppDomain + reflexão), e levar as categorias do Canaltech para a árvore e o arrasto de vários itens sem recompilar a DLL. | ASP.NET Web Forms · .NET Framework 4.7.2 · C# · JavaScript · T-SQL · DevExpress | `Project 12` | ✅ |
| 13 | **Plugins de notícia sem RSS** | Migração de 7 fontes de notícia (G1, iG, iCarros, Gazeta do Povo, Infomoney, TVFoco, Tecmundo) do feed RSS/FTP para a leitura do próprio site (API interna, sitemap, ld+json, og:), em camadas SOLID, sem mudar um byte do que o player legado recebe. | C# · .NET Framework · Newtonsoft.Json · LINQ to XML · NLog · SOLID | `Project 13` | ✅ |
| 14 | **Coleta do UOL sem o XML** | Reconstrução da coleta de 8 seções de notícia e das cotações do UOL depois que o XML congelou: scraping de `ld+json`/Open Graph, navegador Chromium (CEF) atrás de uma interface para furar o 403 da Akamai e API de cotações, com a saída para o player idêntica. | C# · .NET Framework 4.7.2 · CefSharp · LINQ to XML · Newtonsoft.Json · SOLID | `Project 14` | ✅ |
| 15 | **Integrações confiáveis: fim das falhas silenciosas** | Seis correções no pipeline de integrações dos plugins: HttpClient próprio por integração (fim do vazamento da chave da API), log por serviço, descarte por notícia em vez de derrubar o plugin, regra de imagem `media:content` e configuração fora do código. | C# · .NET Framework · HttpClient · NLog · LINQ · ConfigurationManager | `Project 15` | ✅ |

**Legenda:** ✅ documentado · 🚧 em construção
