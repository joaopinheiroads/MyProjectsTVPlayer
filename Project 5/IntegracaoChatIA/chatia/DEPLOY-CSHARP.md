# Deploy — ChatIA (ASP.NET Core .NET 8, MVC)

Backend refeito em C# (substitui o Node/iisnode). Frontend (widget.js/admin.html/chat.html)
e o banco (ChatFaq/ChatLog + procedures) são os mesmos — nada muda no painel nem no banco.

## 1) Pré-requisito no servidor (uma vez)
Instalar o **.NET 8 Hosting Bundle** (runtime ASP.NET Core 8 + módulo do IIS `AspNetCoreModuleV2`):
https://dotnet.microsoft.com/download/dotnet/8.0 → "ASP.NET Core Runtime 8.x" → **Hosting Bundle (Windows)**.
Depois: `net stop was /y && net start w3svc` (ou reiniciar o IIS). Confere com `dotnet --info`.
> Diferente do Node: não precisa de Node/iisnode/npm install. O `web.config` é gerado no publish.

## 2) Publicar (na máquina de dev)
```powershell
cd C:\Users\doutg\OneDrive\Desktop\SandBox\ADM\chatia\ChatIA
dotnet publish -c Release -o .\publish
```
Sobe **todo o conteúdo de `publish\`** via FTP para a pasta do app no servidor
(ex.: `E:\inetpub\chatia` — a mesma usada hoje). Inclui `wwwroot\`, a DLL, e o `web.config`.

## 3) IIS
- Aplicação no site, alias **/chatia**, apontando para a pasta publicada.
- Application Pool em **"No Managed Code"** (o ANCM hospeda o .NET; não é o CLR do IIS).
- O PathBase `/chatia` é tratado automaticamente pelo ANCM (sem mexer em código).

## 4) Segredos (NÃO ficam no appsettings.json em produção)
Definir como variáveis de ambiente do app. Mais simples: no `web.config` publicado,
dentro de `<aspNetCore>`, adicionar:
```xml
<environmentVariables>
  <environmentVariable name="ANTHROPIC_API_KEY" value="sk-ant-..." />
  <environmentVariable name="DB_PASSWORD" value="..." />
  <environmentVariable name="DB_SERVER" value="127.0.0.1" />   <!-- no servidor é 127.0.0.1 -->
</environmentVariables>
```
O resto (CHAT_MODEL, DB_USER, DB_DATABASE, DB_PORT, ADMIN_TOKEN_SECRET) já vem do `appsettings.json`.
Variável de ambiente sempre vence o appsettings.

## 5) Teste
- `https://191.6.5.106:44909/chatia/health` → `{"ok":true,"model":"claude-haiku-4-5"}`
- Abrir o painel → botão 💬 → perguntar algo (ex.: "como crio uma campanha?").
- FAQ admin: só aparece para o admin master (token assinado pelo painel).

## Mapa Node → C# (referência)
| Node                | C#                                   |
|---------------------|--------------------------------------|
| server.js           | Program.cs + Controllers/            |
| claude-client.js    | Services/ClaudeService.cs            |
| prompt-config.js    | Services/PromptConfig.cs             |
| faq-store.js        | Services/FaqStore.cs                 |
| db-log.js           | Services/ChatLogStore.cs             |
| mask.js             | Services/Masker.cs                   |
| auth-admin.js       | Services/AdminAuth.cs                |
| public/*            | wwwroot/*                            |
| .env                | appsettings.json + env (segredos)    |
