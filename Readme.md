# 💻 Portfólio - Dev Full Stack C#

Bem-vindo! Sou Dev Full Stack especializado em **C# / .NET / ASP.NET Core** (e front-end com Blazor, JavaScript e React).

Este repositório reúne projetos reais que desenvolvi durante minha atuação na **TVPlayer / MapMaker**, com o objetivo de comprovar meus trabalhos. Cada projeto tem um **README próprio** com mais detalhes; abaixo está apenas um resumo de cada um.

> 🔒 Este repositório é público e **não contém credenciais, tokens ou acessos a banco de dados**.

---

## 👨‍💻 Sobre Mim

Dev Full Stack com foco em qualidade, performance e código limpo, com experiência desenvolvendo sistemas em produção que atendem milhares de empresas.

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

**Legenda:** ✅ documentado · 🚧 em construção
