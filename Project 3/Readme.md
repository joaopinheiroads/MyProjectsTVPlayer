# Projeto 3 — Cardápio Digital (Site Institucional)

Site institucional / landing page da plataforma de cardápio digital **EscolhaAÍ**. É a vitrine que apresenta o produto, as soluções e os canais de contato, direcionando o visitante para testar a plataforma (Projeto 2).

> Aplicação real da plataforma **EscolhaAÍ**. Repositório público, **sem credenciais** (dados de SMTP/e-mail ficam fora do versionamento).

## Visão geral

Single-page institucional construída em **Blazor WebAssembly**, com seções de apresentação do produto e formulários de contato que disparam e-mail no back-end.

## Principais funcionalidades

- **Landing page** com seções de apresentação: cabeçalho, soluções, demonstração do cardápio e chamadas para ação (*Teste Agora*).
- **Newsletter / captação de contato** — formulário que envia e-mail (com template HTML) pelo back-end.
- **"Ligamos para você"** — solicitação de retorno/callback.
- **Integração com WhatsApp** — direcionamento para atendimento.
- **Navegação por seções** (rolagem/âncoras) via serviço dedicado.

## Stack

- **.NET 8** · **Blazor WebAssembly** (cliente)
- **ASP.NET Core** como host do WASM + endpoint de e-mail
- **MailKit** (envio de e-mail via SMTP, com template HTML)
- **Bootstrap** (estilos)
- Versionado com **SVN (TortoiseSVN)**

## Arquitetura

Solução com dois projetos:

| Projeto | Papel |
|---------|-------|
| `EscolhaAI.Client` | Site em **Blazor WebAssembly**: componentes da página (Header, Soluções, Cardápio, Teste Agora, Newsletter, Footer) e serviços de UI (navegação, WhatsApp). |
| `EscolhaAI` | Host ASP.NET Core que serve o WASM e expõe a API de e-mail (`EmailController` → `EmailService` com MailKit). |

```
CardapioDigitalSite/
├─ EscolhaAI.Client/
│  ├─ Components/        → seções da landing (Header, Solucoes, TesteAgora, Newsletter, ...)
│  ├─ Pages/             → páginas/rotas
│  └─ Services/          → NavigationService, WhatsAppService
└─ EscolhaAI/
   ├─ Controllers/       → EmailController (recebe contato/newsletter)
   ├─ Services/          → EmailService (MailKit)
   ├─ Models/            → NewsletterRequest
   └─ wwwroot/Template/  → template_newsletter.html
```

## Como executar (resumo)

Pré-requisitos: **.NET 8 SDK**.

```bash
# a partir de CardapioDigitalSite/
dotnet run --project "EscolhaAI/EscolhaAI.csproj"
```

Configurar localmente (fora do versionamento) os dados de **SMTP** usados pelo `EmailService` (servidor, porta, usuário e senha) para o envio de e-mails funcionar.
