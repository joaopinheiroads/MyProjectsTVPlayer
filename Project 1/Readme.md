# Projeto 1 — Plugin de Previsão do Tempo

**Primeiro projeto desenvolvido.**

**Demanda:** desenvolver um novo plugin de previsão do tempo para sistema de sinalização digital (mídia indoor).

## Visão geral

Plugin de previsão do tempo feito com **HTML, CSS e JavaScript** puro (sem framework e sem backend em tempo de execução). Todos os dados são recebidos pela **query string** da URL, o que permite que o sistema de sinalização digital injete a previsão e as preferências do cliente diretamente no link da tela.

São oferecidos dois layouts (**MODELO 2** e **MODELO 3**), cada um com versão de **1 dia** e de **4 dias**, para que o usuário escolha a apresentação que preferir.

## Diferenciais

1. **Layout dinâmico conforme a previsão (ou definido pelo usuário).** O usuário pode fixar uma camada de cores para o seu layout, ou deixar automático para que o plano de fundo/cores mudem conforme a própria previsão — tudo via query string.
2. **Multi-idioma** (português e inglês), também via query string.
3. **Layout altamente responsivo** para telas **4K, FHD, HD e painéis de LED**, tanto na orientação **horizontal** quanto **vertical**.

> **MODELO 3** contém basicamente o mesmo sistema do **MODELO 2**, porém com um layout totalmente diferente, dando mais uma opção de escolha ao usuário.

## Tecnologias

- **HTML5 / CSS3 / JavaScript** (vanilla, sem dependências de build)
- **Weather Icons** (fonte de ícones de clima)
- Ícones de condição climática baseados nos **códigos do Yahoo Weather** (`yCode`)
- Versionado com **SVN (TortoiseSVN)**

## Estrutura

```
PluginPrevisaoDoTempo/
├─ MODELO 2/
│  ├─ 1dia/    → tela de 1 dia  (index1d.html + clima_1d.js)
│  ├─ 4dias/   → tela de 4 dias (index4d.html + script4d.js)
│  ├─ assets/  → imagens de fundo por condição (HD/FH, horizontal/vertical)
│  └─ fonts/   → fontes e Weather Icons
└─ MODELO 3/   → mesma estrutura, layout alternativo
```

## Como usar

Basta abrir o HTML correspondente (servido por qualquer servidor estático) passando os parâmetros na URL. Exemplos prontos estão em **`MODELO 2/QueryString.txt`**.

Exemplo (1 dia):

```
index1d.html?cidade=Londrina/UF&yCode=32&dt=01/01/2017&max=35&min=20
  &minV=2&maxV=10&minU=10&maxU=50&clima=tempo%20bom&modelo=2
  &corBg1=908080&corBg2=000000&corFonte=ffffff&direcao=&unidadeTemp=c
  &custombg=custombg.jpg&idioma=ptbr
```

Principais parâmetros:

| Parâmetro | Descrição |
|-----------|-----------|
| `cidade` | Nome da cidade exibido |
| `yCode` | Código de condição do tempo (Yahoo) — define ícone e fundo |
| `clima` | Texto da condição climática |
| `dt` | Data (1 ou várias, separadas por vírgula no modelo de 4 dias) |
| `max` / `min` | Temperatura máxima / mínima |
| `minV` / `maxV` | Velocidade do vento (mín/máx) |
| `minU` / `maxU` | Umidade (mín/máx) |
| `modelo` | Modo de renderização (1/2/3) |
| `idioma` | `ptbr` ou `eng` |
| `unidadeTemp` | `c` (Celsius) ou `f` (Fahrenheit) |
| `corBg1` / `corBg2` / `corFonte` | Cores de fundo e fonte (modo manual) |
| `direcao` | Direção do gradiente: `h` (horizontal) ou `v` (vertical) |
| `custombg` | Imagem de fundo personalizada (MODELO 3) |
