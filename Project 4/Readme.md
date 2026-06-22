# Projeto 4 — Integração com o Kommo CRM

> **Escopo:** esta pasta contém **apenas o módulo de integração com o Kommo CRM** que eu desenvolvi, extraído da solução do site TV Player. O site completo **não** faz parte deste repositório.

## O que é

Integração que mantém o **funil de vendas do Kommo CRM sincronizado automaticamente** com o estado real do cliente no sistema TVPlayer. Quando um lead vira cliente (ou entra em período de teste), ele é **movido automaticamente para o estágio correto do funil** no Kommo — sem intervenção manual da equipe comercial.

A integração é bidirecional no sentido do fluxo de informação:
- **Kommo → Sistema:** recebe **webhooks** do Kommo (ex.: criação de contato) e registra os leads localmente.
- **Sistema → Kommo:** chama a **API REST v4 do Kommo** para mover o lead de estágio no funil.

## Como funciona

A integração vive em `TVPlayerSite.API` e gira em torno de `KommoController` (rota `api/Kommo`) e `KommoService`.

### 1. Recebimento de webhook (Kommo → Sistema)
`POST api/Kommo/ReceiveWebHook` (anônimo)
- O Kommo dispara o webhook (form-urlencoded) quando um **contato é criado** (`contacts[add]`).
- O controller extrai `leadId`, `contactId`, nome, telefone e e-mail do payload.
- O lead é **persistido** localmente (tabela `Leads`).
- O telefone é **normalizado** (remove `+55`, espaços, parênteses, hífens) para casar com o cadastro.
- O serviço tenta **vincular o lead a um usuário** existente (por e-mail ou celular). Se encontrar, verifica o **terminal/grupo** do usuário e decide o destino no funil:
  - `GrupoTi == 1` → estágio **Cliente**
  - `GrupoTi == 3` → estágio **Testando**
- Em seguida, move o lead no Kommo.

### 2. Atualização de grupo (Sistema → Kommo)
`POST api/Kommo/AtualizarGrupo` (autenticado)
- Recebe um `GrupoId`; localiza o usuário daquele grupo e o lead correspondente (por e-mail/telefone) e o move para o estágio **Cliente** no funil.

### 3. Mover lead manualmente (utilitário)
`GET api/Kommo/mover-lead?leadId={id}&statusId={status}`
- Move diretamente um lead para um estágio específico — útil para testes e correções pontuais.

### 4. Diagnóstico
`GET api/Kommo/Test` — confirma se a integração está no ar e se as credenciais do Kommo estão configuradas.

> Comunicação com o Kommo: **`PATCH /api/v4/leads`** com autenticação **Bearer** (token de acesso de longa duração). Os IDs de estágio do funil (ex.: *Cliente*, *Testando*) são mapeados como constantes no serviço.

## Stack

- **ASP.NET Core Web API** (.NET) — padrão Controller + Service
- **HttpClient** via `IHttpClientFactory` (chamadas à API v4 do Kommo)
- **Newtonsoft.Json** (serialização do payload)
- **Entity Framework Core** (persistência dos leads recebidos)
- **ILogger** (rastreabilidade detalhada de cada etapa do fluxo)
- Versionado com **SVN (TortoiseSVN)**

## Arquitetura (arquivos principais)

> Esta pasta contém **apenas os arquivos da integração Kommo** que desenvolvi, extraídos da solução do site TV Player (o site completo não faz parte deste repositório).

```
IntegracaoKommo/
├─ Controllers/KommoController.cs   → endpoints (webhook, mover lead, atualizar grupo, test)
├─ Services/KommoService.cs         → regra de negócio da integração
├─ Interfaces/IKommoService.cs
├─ Interfaces/ILeadRepository.cs
├─ Repositories/LeadRepository.cs   → persistência de leads
├─ Models/Leads.cs                  → entidade Leads (LeadId, ContactId, Nome, Email, Telefone, CriadoEm)
└─ DTO/
   ├─ GrupoAtualizadoDTO.cs
   └─ WebHookDTO.cs                 → WebhookPayloadDTO (payload recebido do Kommo)
```

Registro na injeção de dependência (no `Startup` da API original):

```csharp
services.AddScoped<IKommoService, KommoService>();
services.AddScoped<ILeadRepository, LeadRepository>();
```

Fluxo: `KommoController` → `IKommoService` (`KommoService`) → `ILeadRepository` + `DbContext` (leitura de usuário/terminal e gravação do lead) → **API v4 do Kommo**.

## Configuração

Definir **fora do versionamento** (em user-secrets / variáveis de ambiente):

| Chave | Descrição |
|-------|-----------|
| `Kommo:BaseUrl` | URL base da conta no Kommo |
| `Kommo:AccessToken` | Token de acesso (Bearer) da API do Kommo |

> ⚠️ Nenhuma credencial do Kommo é versionada neste repositório.

## Relação com outros projetos

Os **Projetos 7 e 8** vieram da mesma solução (site TV Player) e já estão documentados em suas próprias pastas:
- **Projeto 7** — tela e back-end do **código de verificação** no cadastro de demonstração.
- **Projeto 8** — **verificador de demonstrações** a vencer/vencidas com disparo de mensagens.
