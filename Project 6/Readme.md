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

## Observações de segurança

O servidor sobe em **HTTPS** lendo um certificado `cert.pfx` com uma *passphrase*. Para um repositório público:

- **Não versionar o `cert.pfx`** (certificado/segredo) — deve ser provido apenas no servidor.
- **Mover a passphrase para variável de ambiente** (hoje ela está no código).
- Adicionar um **`.gitignore`** ignorando `node_modules/`, `dist/` e `cert.pfx`.
