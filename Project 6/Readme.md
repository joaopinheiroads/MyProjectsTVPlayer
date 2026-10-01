# Projeto 6 — Plugin Impostômetro

Plugin que exibe, em tempo real, o valor do **Impostômetro** (total de impostos arrecadados no Brasil) para exibição em **sinalização digital / mídia indoor**. Como o site oficial não oferece API, o valor é obtido por **web scraping** e disponibilizado por uma API própria, consumida por uma tela leve.

## Como funciona

- Um **worker** em Node usa o **Puppeteer** (navegador headless) para abrir `impostometro.com.br`, aguardar o contador (`#counterBrasil`) e extrair o valor já formatado (milhares/centavos).
- O valor fica em **cache em memória** e é **atualizado automaticamente a cada 5 minutos**.
- Uma **API Express** expõe o endpoint **`GET /impostometro`**, que devolve `{ valor, atualizadoEm }`.
- O **front-end** (`index.html` + CSS) consome a API e exibe o número na tela.

## Stack

- **Node.js** · **TypeScript**
- **Express 5** (API)
- **Puppeteer** (scraping de página dinâmica) — também há `axios`/`cheerio` disponíveis
- **CORS**
- **SCSS/Sass** (estilos → `style.css`)
- **PM2** (manter o serviço no ar em produção / Windows)

## Estrutura

```
Impostometro/
├─ scraper.ts        → worker (Puppeteer) + API Express (GET /impostometro)
├─ index.html        → tela que exibe o valor
├─ styles.scss       → estilos (compilados para style.css)
├─ tsconfig.json     → configuração do TypeScript
└─ package.json      → scripts e dependências
```

## Como executar

Pré-requisitos: **Node.js**.

```bash
# instalar dependências
npm install

# desenvolvimento (executa o TypeScript direto)
npm run dev

# build + produção
npm run build
node scraper.js
```

Em produção, o serviço é mantido com **PM2** (reinício automático):

```bash
pm2 start scraper.js --name impostometro
pm2 save
pm2 status
```

## Atualização (set/2026) — TLS saiu do Node e foi para o IIS

**Antes**, o servidor subia em HTTPS lendo um `cert.pfx` de dentro da pasta do projeto, com a *passphrase* escrita no código — e toda renovação de certificado virava alteração de código + build + restart do PM2.

> O código publicado nesta pasta ainda é a versão **anterior** à migração (com `https.createServer` e a passphrase já removida).

**Depois**, a criptografia termina no IIS (**TLS termination**) e o Node fala só HTTP, preso ao loopback:

```
Navegador --HTTPS 3001--> IIS (certificado no repositório do Windows)
                            |  proxy reverso (URL Rewrite + ARR), HTTP puro
                            v
                          Node (scraper.js) em 127.0.0.1:3000
                            |  Puppeteer, a cada 5 min
                            v
                          impostometro.com.br
```

- O `https.createServer(httpsOptions, app)` saiu do `scraper.ts`; o `app.listen(PORT, '127.0.0.1')` garante que a porta do Node **não fica exposta na rede** — só o IIS fala com ela.
- O trecho em claro é aceitável porque IIS e Node estão na **mesma máquina** (loopback).
- O `cert.pfx` e a senha saíram do projeto; renovar certificado agora é tarefa do IIS, sem build.
- O endpoint consumido pela tela **não mudou** — quem atende na porta é que trocou de dono.
- Rota `/health` mantida, devolvendo o horário da última coleta (`atualizadoEm`), usada para provar que o scraping está em dia.

**O diagnóstico do deploy** ("tela parada em R$ CARREGANDO...") foi feito de fora, lendo a diferença entre as respostas TCP: porta 80 respondeu (IIS vivo); a 3001 devolveu **RST** (pacote chegou, ninguém escutando → faltava o binding no IIS, firewall descartado); 3000/443 deram **timeout** (firewall descartando, como deveria). Como o `Test-NetConnection` devolve `False` nos dois casos, foi usado um `TcpClient` lendo o `SocketException.ErrorCode` (`10061` = conexão recusada).

**Validação final** com verificação de certificado ligada: handshake TLS 1.2 sem aviso, `/health` e `/impostometro` respondendo pelo proxy.

## Observações de segurança

- `cert.pfx` e *passphrase* **não existem mais** no projeto — o certificado vive no repositório de certificados do Windows, gerenciado pelo IIS.
- O Node escuta apenas em `127.0.0.1`.
- `.gitignore` ignora `node_modules/`, `dist/` e arquivos de certificado.
