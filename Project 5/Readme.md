# Projeto 5 — Chat com IA (Claude) para Suporte Interno

Assistente virtual com **IA da Anthropic (Claude)** embutido no painel administrativo da plataforma de mídia indoor / sinalização digital, na forma de um **widget de chat**. Responde, em tempo real, dúvidas dos usuários sobre **como usar o sistema** (onde fica cada função e como configurar), com base em uma **base de conhecimento curada** — sem expor dados de clientes.

> Projetado e desenvolvido de ponta a ponta: back-end, engenharia de prompt, base de conhecimento, widget e deploy. Originalmente um back-end em Node, foi **reescrito em C#/.NET**.

## Destaques

- **Streaming de resposta (SSE):** a resposta é transmitida token a token via *Server-Sent Events*, com efeito "digitando".
- **Prompt caching:** o *system prompt* = **Persona + Base de Conhecimento**, com a base marcada para cache (`CacheControlEphemeral`), reduzindo **custo e latência** nas mensagens seguintes.
- **Persona com proteção (anti-jailbreak):** escopo restrito ao painel; recusa pedidos fora de tema; **não revela** o próprio prompt; **white-label** (nunca cita marca/fornecedor); responde no idioma do usuário (PT/ES/EN); formato enxuto; suporte a vídeo-tutorial via marcador.
- **Não expõe dados sensíveis:** a IA é instruída a nunca informar dados operacionais ou de clientes (players, faturamento, relatórios, dados de empresas) — apenas orienta onde o próprio usuário encontra no painel.
- **Mascaramento de PII nos logs:** antes de gravar o histórico, e-mail, CPF, CNPJ, cartão, telefone e senhas/tokens são mascarados (`Masker`).
- **Base de conhecimento gerenciável:** vem do banco de dados (FAQ) com **fallback** para `base-conhecimento.md` quando o banco está vazio/indisponível; há um **painel admin** para gestão.
- **Widget injetável:** `widget.js` em JavaScript puro (vanilla), embutível em qualquer página; CORS aberto para rodar em outra origem.

## Stack

- **.NET 8** · **ASP.NET Core** (Web API)
- **SDK oficial Anthropic** (pacote `Anthropic`) — modelo padrão **`claude-haiku-4-5`** (configurável via `CHAT_MODEL`)
- **Server-Sent Events (SSE)** para streaming
- **SQL Server** (`Microsoft.Data.SqlClient`) para logs e base de conhecimento
- **JavaScript** (widget vanilla)
- Versionado com **SVN (TortoiseSVN)**

## Arquitetura

```
chatia/
├─ Controllers/
│  ├─ ChatController.cs     → POST /api/chat (responde em streaming via SSE)
│  └─ FaqController.cs      → administração da base de conhecimento (FAQ)
├─ Services/
│  ├─ ClaudeService.cs      → integração com o Claude (system = Persona + Base cacheada)
│  ├─ PromptConfig.cs       → Persona/regras (anti-jailbreak, white-label) + fallback da base
│  ├─ FaqStore.cs           → base de conhecimento (banco)
│  ├─ ChatLogStore.cs       → log das perguntas (mascaradas)
│  ├─ Masker.cs             → mascaramento de dados sensíveis (PII)
│  ├─ AdminAuth.cs          → autenticação do painel admin
│  └─ Db.cs                 → acesso ao SQL Server
├─ wwwroot/
│  ├─ widget.js             → widget de chat injetável
│  ├─ chat.html             → página de chat
│  └─ admin.html            → painel de gestão da base de conhecimento
├─ base-conhecimento.md     → base de fallback
└─ Program.cs               → DI, CORS, arquivos estáticos, /health
```

Fluxo de uma mensagem: `widget.js` → `POST /api/chat` (`ChatController`) → `ClaudeService.StreamAsync` (monta system com Persona + base cacheada e faz streaming) → resposta devolvida em SSE; a pergunta é registrada em log de forma mascarada e assíncrona.

## Configuração

O repositório inclui **`appsettings.example.json`** como modelo. Copie para `appsettings.json` (que é **ignorado pelo Git** via `.gitignore`) e preencha:

| Chave | Descrição |
|-------|-----------|
| `ANTHROPIC_API_KEY` | Chave da API da Anthropic (ou usar a variável de ambiente de mesmo nome) |
| `CHAT_MODEL` | Modelo do Claude (padrão `claude-haiku-4-5`) |
| `DB_SERVER` / `DB_PORT` / `DB_DATABASE` / `DB_USER` / `DB_PASSWORD` | Conexão com o SQL Server |
| `ADMIN_TOKEN_SECRET` / `ADMIN_USER_IDS` | Autenticação do painel admin |

## Como executar (resumo)

Pré-requisitos: **.NET 8 SDK** e **SQL Server** (opcional em dev — sem banco, usa a base de fallback).

```bash
# a partir de IntegracaoChatIA/chatia/
dotnet run
# health check: GET /health  → { ok, model }
```
