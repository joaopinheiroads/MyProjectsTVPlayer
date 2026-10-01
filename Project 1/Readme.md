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

## Atualização (set/2026) — diagnóstico de produção: o gzip velho do IIS

**Sintoma:** a previsão de 1 dia abria certa na primeira vez e perdia toda a formatação no F5.

**Investigação:** o CSS chegava inteiro, mas o **HTML** mudava: na primeira abertura vinha o `index1d.html` do MODELO 2; do reload em diante, o do MODELO 3 (outra marcação, em cima do CSS errado). A prova veio com `curl`, variando só o cabeçalho de compressão:

```bash
curl -sk              "$URL/index1d.html"   # sem gzip -> MODELO 2, 4353 bytes (certo)
curl -sk --compressed "$URL/index1d.html"   # com gzip -> MODELO 3, 5061 bytes (errado)
```

**Causa:** um `index1d.html` do MODELO 3 tinha sido copiado para a pasta por engano e o IIS guardou a versão comprimida dele no cache de compressão estática. O arquivo certo voltou com a **mesma data de modificação** (a cópia do Windows preserva a data de origem), e o IIS continuou servindo o gzip antigo. A primeira abertura saía certa porque a compressão estática só entra para arquivos pedidos com frequência (`frequentHitThreshold`).

**Correção:** atualizar a data do arquivo, forçando o IIS a refazer o gzip — sem tocar em código:

```powershell
(Get-Item $f).LastWriteTime = Get-Date
```

No reteste, 10 de 10 respostas com gzip e 5 de 5 sem gzip vieram com a versão certa.

**Achado de quebra:** se a URL viesse sem o parâmetro `idioma`, o `MyDate` lia `this.idioma[undefined]` e lançava `TypeError`, parando o script no meio. A correção (idioma padrão normalizado com `indexOf` + ternário, aplicada nos quatro scripts) foi preparada, publicada e depois **revertida a pedido** enquanto se investigava um erro de rede nos players Android — sem relação com o JS.

**Aprendizados:** isolar a variável (com/sem `Accept-Encoding`) antes de mexer em código; data de arquivo não prova versão; para reverter, copiar o backup inteiro em vez de editar de volta (um `sed -i` trocou CRLF por LF e o arquivo deixou de ser idêntico byte a byte).
