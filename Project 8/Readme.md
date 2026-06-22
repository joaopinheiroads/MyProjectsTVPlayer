# Projeto 8 — Verificador de Demonstrações

Rotinas de **back-end em segundo plano** que monitoram as contas em **período de demonstração** do TV Player e **disparam mensagens de WhatsApp** automaticamente — avisando o cliente quando a demonstração está **a vencer / vencida** e incentivando a **contratação**, além de re-engajar quem começou o teste e parou.

> **Back-end desenvolvido por mim.** Módulo **extraído do Projeto 4** (site TV Player) — aqui estão apenas os arquivos desta funcionalidade.

## O que faz

São dois serviços de background (`IHostedService` com `Timer`), que rodam sozinhos junto com a aplicação:

### 1. `VerificadorDeDemonstracoes` — alerta de vencimento
- Executa **1x por dia** (às 10h), de forma idempotente (não repete no mesmo dia).
- Busca os **terminais em demonstração** (grupo de demonstração, ativos, com data de instalação).
- Calcula os **dias desde a instalação** e compara com a duração da demonstração (**padrão 10 dias**, configurável por terminal via `TerminalQtdDiasDemonstracao`).
- Identifica quem está no **penúltimo dia** e no **último dia** e envia o WhatsApp adequado:
  - **Penúltimo dia:** "sua demonstração está chegando ao fim — solicite a contratação no Painel".
  - **Último dia:** "estamos encerrando o teste — se ainda tiver interesse, reativamos sua conta".
- Em caso de **falha no envio**, notifica o **número comercial** para acompanhamento manual.
- Grava **log em arquivo** com rotação automática (remove logs com mais de 30 dias).

### 2. `DisparoDemonstracao` — re-engajamento
- Executa **1x por dia** (às 10h).
- Busca clientes cadastrados nos **últimos 2 dias** que **não confirmaram o e-mail** e ainda **não receberam** este WhatsApp.
- Envia uma mensagem convidando a continuar o teste e conhecer melhor a plataforma.
- **Marca o cliente** como já contatado (`DisparoWhatsApp = 1`) para não repetir.

## Tecnologias

- **.NET / ASP.NET Core** — serviços de segundo plano (`IHostedService` + `Timer`), agendamento diário
- **Entity Framework Core** (consultas de terminais/usuários) + **ADO.NET / SqlClient** (consultas e updates pontuais)
- **Integração WhatsApp** via **API ChatPro** (`DisparosChat`)
- **Logging em arquivo** com rotação (30 dias)
- Versionado com **SVN (TortoiseSVN)**

## Estrutura (arquivos deste módulo)

```
VerificadorDemonstracao/
├─ VerificadorDeDemonstracoes.cs            → alerta de vencimento (penúltimo/último dia)
├─ DisparoDemonstracao.cs                   → re-engajamento (cadastro sem e-mail confirmado)
├─ DisparosChat.cs                          → cliente da API de WhatsApp (ChatPro)
├─ Logs/VerificadorDeDemonstracoesLogger.cs → log em arquivo (rotação de 30 dias)
└─ Models/TerminalQtdDiasDemonstracao.cs    → dias de demonstração configuráveis por terminal
```

Registro (no projeto original): os serviços são adicionados como *hosted services* na inicialização da API (`AddHostedService<VerificadorDeDemonstracoes>()` / `AddHostedService<DisparoDemonstracao>()`), passando a rodar continuamente em background.

## Observação de segurança

> 🔒 O **token da API de WhatsApp (ChatPro)** e o **ID da instância** que estavam embutidos no `DisparosChat.cs` foram **substituídos por `[TOKEN_CHATPRO_REMOVIDO]` / `[INSTANCIA_REMOVIDA]`** nesta cópia. Em produção, essas credenciais devem vir de configuração/variável de ambiente — nunca versionadas.
