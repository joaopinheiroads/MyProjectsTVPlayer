# Projeto 7 — Código de Verificação (cadastro de demonstração)

Tela e back-end do **código de verificação** exigido ao se cadastrar no **formulário de demonstração** do site TV Player. Quando o visitante solicita uma demonstração, o sistema gera um código, envia por **WhatsApp e e-mail**, e só conclui o cadastro após o visitante confirmar o código — reduzindo cadastros falsos/bots.

> **Front-end e back-end desenvolvidos por mim.** Este módulo foi **extraído do Projeto 4** (site TV Player) — aqui estão apenas os arquivos da funcionalidade de código de verificação, não o site inteiro.

## Fluxo

1. **Formulário de demonstração** (`Index.cshtml`): o visitante informa nome, empresa, e-mail, telefone, etc., com **reCAPTCHA** obrigatório.
2. **Envio do código** (`POST .../demonstracao`): valida o reCAPTCHA, **gera um código de 4 dígitos**, guarda os dados + código + horário na **sessão** (válido por **5 minutos**) e envia o código por:
   - **WhatsApp** (serviço interno de disparo);
   - **E-mail** (API `DemonstracaoController` → `DemonstracaoService` → `EmailHelper`, com template HTML).
   - A resposta abre, via AJAX, o **modal de verificação**.
3. **Verificação** (`POST .../demonstracao/verificar-codigo-ajax`): confere se o código existe, está dentro dos 5 minutos e confere. Se correto:
   - cria a **licença de demonstração**;
   - verifica se o e-mail já existe (via API) e trata cliente novo × existente;
   - salva o cliente e dispara **e-mail de confirmação** + **WhatsApp de boas-vindas** com link de validação de e-mail;
   - registra cada etapa em log e limpa a sessão.
4. **Reenvio**: `reenviar-codigo` (WhatsApp) e `reenviar-codigo-email` (e-mail).
5. **Validação de e-mail** (`ValidarEmail`): confirma o e-mail pelo link e dispara os **procedimentos de instalação** (login, senha e chave de ativação) por e-mail e WhatsApp.

## Tecnologias

- **ASP.NET Core MVC** (.NET) — Views Razor (`.cshtml`), controllers e **sessão** server-side
- **JavaScript / jQuery + AJAX** (modal de verificação, sem recarregar a página)
- **reCAPTCHA** (Google) — proteção anti-bot
- **Antiforgery token** (`RequestVerificationToken`)
- **WhatsApp** via serviço interno de disparo
- **E-mail SMTP** (`System.Net.Mail`) com templates HTML
- **API REST** (`TVPlayerSite.API`) para o envio do código por e-mail
- **Internacionalização** (Resources `.resx` PT/EN/ES)
- **Criptografia** para os links de confirmação/contratação
- **Logging dedicado** do processo (`LogVerificacaoHelper`)

## Decisões de segurança do recurso

- Código **numérico de 4 dígitos** com **expiração de 5 minutos**.
- Código guardado **apenas na sessão do servidor** (nunca trafega para o cliente).
- **reCAPTCHA** obrigatório antes de gerar o código.
- **Antiforgery token** nas requisições.
- Log de tentativas (sucesso/erro) para auditoria.

## Estrutura (arquivos deste módulo)

```
CodigoVerificacao/
├─ Frontend/   (site MVC — TVPlayerSite)
│  ├─ Controllers/DemonstracaoController.cs   → fluxo (gerar, verificar, reenviar, validar e-mail)
│  ├─ Views/Demonstracao/Index.cshtml         → formulário de demonstração + modal
│  ├─ Views/Demonstracao/VerificacaoCodigo.cshtml → tela de digitação do código
│  ├─ wwwroot/css/demonstracao.css            → estilos
│  ├─ Helpers/EmailHelper.cs                  → envio de e-mails (templates)
│  ├─ Helpers/LogVerificacaoHelper.cs         → log do processo
│  ├─ Classes/CaptchaVerification.cs          → resposta do reCAPTCHA
│  └─ Resources/ResourceDemonstracao.*        → textos PT/EN/ES
└─ Backend/    (API — TVPlayerSite.API)
   ├─ Controllers/DemonstracaoController.cs    → POST api/Demonstracao/enviar-codigo
   ├─ Services/DemonstracaoService.cs          → regra de envio do código por e-mail
   ├─ Interfaces/IDemonstracaoService.cs
   ├─ Helpers/EmailHelper.cs
   ├─ Helpers/LogVerificacaoHelper.cs
   └─ Models/EnviarCodigoRequest.cs            → { Email, Nome, Codigo }
```

## Observação de segurança

> 🔒 A **senha do SMTP** que estava embutida no `EmailHelper.cs` foi **substituída por `[SENHA_SMTP_REMOVIDA]`** nestas cópias. Em produção, essa credencial deve vir de configuração/variável de ambiente — nunca versionada.
