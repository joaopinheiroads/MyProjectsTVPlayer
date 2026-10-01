# Projeto 14 — Coleta do UOL sem o XML (scraping, navegador CEF e API de cotações)

Reconstrução da coleta de conteúdo do UOL depois que o portal **parou de atualizar o XML de mídia indoor** que alimentava a TV Player: as **8 seções de notícia** passaram a ser lidas da própria página do site, as **3 seções barradas com 403** pela borda da Akamai passaram por um **navegador Chromium embarcado (CEF)** atrás da mesma interface, e as **cotações** saíram de um XML congelado havia 15 meses para a **API que a própria página do UOL consome**.

> **Mudou só a coleta, a entrega ficou idêntica.** O scraper devolve exatamente o mesmo XML intermediário (`rss/channel/item`) que a transformação XSLT antiga produzia, então merge, download de imagem, os arquivos da pasta e o **player instalado nos terminais** continuaram recebendo o mesmo formato, sem nenhuma atualização do lado do cliente. O código está em produção.

## O problema

O XML de mídia indoor do UOL **continuava respondendo `200 OK`, mas tinha parado no tempo**: os destaques congelaram em 04/07/2025, os vídeos em 01/02/2024 e as cotações em **30/06/2025**. Como o status HTTP era sempre de sucesso, nenhum log acusava nada.

- **Notícia velha em silêncio** — as seções de notícia exibiam o mesmo conteúdo por meses, sem erro nenhum registrado.
- **Número financeiro errado na tela** — a tela de câmbio mostrava o **dólar travado em R$ 5,44** (cotação de 30/06/2025).
- **403 intermitente** — as seções de `noticias.uol.com.br` (Política, Cotidiano, Internacional) eram barradas pela Akamai. Trocar o `User-Agent` não mudava nada (3 variantes, resultado idêntico em 10 rodadas); a causa provada foi o **fingerprint de TLS/HTTP2** do `HttpClient`, que não se parece com um navegador.
- **Contrato rígido do outro lado** — o player instalado nos terminais não pode ser atualizado. Os nomes dos arquivos, os campos, a ordem e até a posição de cada linha de cotação são contrato.

## A solução

Uma pasta por fonte, com uma responsabilidade por arquivo:

| Camada | Arquivo | Responsabilidade |
|--------|---------|------------------|
| **Interface** | `IFonteHtml` | O contrato mínimo: `ObterHtmlAsync(url)` e `Nome`. Não diz *como* o HTML é obtido. |
| **Client (barato)** | `FonteHttp` | `HttpClient` com 2 tentativas e renovação da conexão (`ServicePoint`) entre elas. |
| **Client (caro)** | `NavegadorCef` | Chromium off-screen (CefSharp); espera o evento de carregamento, lê o status HTTP do frame principal e cancela imagem, CSS, fonte e mídia. |
| **Composição** | `FontesHtml` | Escolhe a fonte por seção; o navegador é **registrado de fora**, no arranque do serviço. |
| **Concorrência** | `CefCompartilhado` | `Cef.Initialize` único e um `SemaphoreSlim(1,1)` que serializa UOL e Loteria no mesmo CEF. |
| **Model** | `UOLSecao` | As seções **como dado**: URLs de listagem, host esperado, categoria e `.ComNavegador()`. |
| **Parser/Scraper** | `UOLScraper` | `ld+json` e metatags `og:` → item; escolhe a maior variante de imagem que responde; monta o XML de encaixe. |
| **Helper** | `DateTimeHelper` | Um único ponto que escreve o `pubDate` em UTC com fuso numérico (`+0000`). |

E as cotações, na mesma forma em camadas:

| Camada | Arquivo | Responsabilidade |
|--------|---------|------------------|
| Interface | `IFonteCotacoesUOL` | `ObterMoedaAsync` / `ObterIndiceAsync`. |
| Client | `CotacoesUOLApiClient` | Só o transporte até a API de cotações do site. |
| Parser | `ResumoCotacaoParser` | JSON → `CotacaoUOL`; prende a hora da fonte ao fuso de Brasília antes de converter para UTC. |
| Model | `IndicadorCotacao`, `TipoIndicador`, `CotacaoUOL`, `LinhaCotacao` | A **ordem da tela como dado** (`NaOrdemDaTela`) e a compra como `decimal?`, porque índice de bolsa não tem compra. |
| Service | `UOLCotacoesService` | A regra: busca tudo, **tudo ou nada**, e monta as 8 linhas na ordem do contrato. |
| Saída | `EscritorCotacoesXml` | O XML `rss/channel/item` de sempre. |

### Decisões que valem destacar

- **Polimorfismo no lugar de `if`.** O `UOLScraper` só conhece `IFonteHtml`. Qual corpo roda — `FonteHttp` ou `NavegadorCef` — é decidido em tempo de execução pelo objeto registrado. Mesmo **contrato de falha** nas duas: `string.Empty`, nunca exceção.
- **Inversão de dependência entre projetos.** A biblioteca (`TVPlayerAPI`, AnyCPU) define o contrato; o serviço (`TI.TVPlayer.Service`, x86, que já tinha o CefSharp) traz a implementação e a registra numa linha no *composition root*. A biblioteca não passou a carregar os 86 MB do `libcef.dll`.
- **Degradação graciosa.** Sem navegador registrado, `FontesHtml.Para` devolve o HTTP e o sistema continua de pé.
- **Open/Closed.** Seção nova, ou seção que passou a ser bloqueada, é **uma linha** em `UOLSecao.Ativas`. Indicador novo é uma linha em `IndicadorCotacao.NaOrdemDaTela`.
- **Não se chuta a dimensão da imagem.** Medido em 11 fotos, `1920x1080` existia em só 2 e `1200x675` em 9; fixar a maior derrubava a seção inteira. O scraper monta as candidatas com **regex de grupos nomeados** (`(?<w>\d{2,4})x(?<h>\d{2,4})`, com *lookahead* negativo para `.webp`) e confere cada uma com `HEAD`, com teto de 6.
- **Placeholder não é foto.** Matéria sem foto própria recebe o logo do UOL no `og:image`; a ausência do sufixo `_NNNxNNN` identifica o placeholder e o item é descartado.
- **Falha de seção é `SemConteudo`, não `Error`.** A pasta anterior fica intacta e o merge cobre o ciclo seguinte; `Error` no log voltou a significar problema real.

### A correção do `pubDate` (+6h)

Depois da migração, as matérias do UOL apareciam **exatamente 6 horas no futuro** (publicada às 12:28 UTC, gravada como 18:28 UTC). Eram **dois erros de +3h somados**:

1. **Na leitura** — o Newtonsoft já entregava o `datePublished` como `DateTime` com `Kind = Utc`; o cast para `string` jogava fora o `Z`, e o `TryParse(..., AssumeLocal)` relia aquilo como hora de Brasília. Correção: ler o `DateTime` que já vinha pronto, sem ir e voltar por texto.
2. **Na escrita** — `ToString("r")` gera `GMT`, que é RFC-822 válido, mas a biblioteca que lê o XML adiante tratava a hora como local e somava o fuso de novo. Correção: `DateTimeHelper.PubDateRssEmUtc`, que normaliza o `Kind` e escreve `+0000`. Mexer no leitor consertaria na raiz, mas ele é o caminho de **todos** os plugins — e a saída é contrato.

Como o merge compara o `pubDate` formatado, o deploy incluiu a limpeza dos arquivos de merge das pastas afetadas para os itens não duplicarem.

## Resultado

| | Antes | Depois |
|---|---|---|
| Fonte das 8 seções de notícia | XML congelado desde jul/2025 (0% de chance de atualizar) | Página do site, a cada ciclo |
| Seções de `noticias.uol.com.br` | 403 na maioria dos ciclos (~20% passavam) | **200 pelo navegador, 403 no `HttpClient` no mesmo minuto — 3 de 3** |
| Dólar comercial na tela | **R$ 5,44** (30/06/2025) | **R$ 5,20** (cotação real de 28/09/2026, com compra e venda) |
| `pubDate` das matérias | **+6h no futuro** (15 matérias afetadas) | Hora real, 0 duplicatas após o deploy |
| Formato entregue ao player | — | **Idêntico**, sem atualizar nenhum terminal |

- **Política ponta a ponta pelo navegador:** `Sucesso` com **10 itens em 39 s** (~3,5 s por navegação); custo do ciclo ~14 navegações, ~2 min.
- **Filtro de recursos sem perda:** bloquear imagem, CSS, fonte e mídia (a maior parte dos ~680 KB por página) rendeu **exatamente os mesmos links** que a página completa, medido no mesmo minuto. JavaScript e XHR ficam liberados, porque o desafio da Akamai roda em JS.
- **Primeiro ciclo em produção:** Esporte 6 itens, Entretenimento 15, Celebridades 10, Televisão 11, Economia 10 — **100% com crédito da foto**.
- **Cotações validadas byte a byte:** os 4 arquivos da pasta saíram com os mesmos campos, a mesma ordem e a mesma acentuação da produção; a tela de câmbio foi aprovada no player de teste. *(No fim do mesmo dia, a empresa decidiu aposentar o plugin de cotações; a coleta foi retirada do código sem remover os valores do `enum` que o player conhece.)*

## Tecnologias e conceitos

- **C#** · **.NET Framework 4.7.2** · serviço Windows em WPF
- **Web scraping** — `ld+json`, Open Graph, **regex com grupos nomeados** e *lookahead*, `Regex.Escape`
- **Iteradores com `yield return`** e recursão (`Achatar` para `ld+json` aninhado) · **LINQ** · **LINQ to XML** (`XElement`/`XDocument`)
- **CefSharp (Chromium off-screen)** — `TaskCompletionSource` como ponte evento → `await`, `Task.WhenAny` para timeout, `DefaultRequestHandler` com `override`
- **Concorrência** — `SemaphoreSlim` + `IDisposable`/`using` como reserva, `lock` na inicialização
- **SOLID** — SRP (uma camada por arquivo), OCP (seção e indicador como dado), LSP (fontes intercambiáveis), ISP (interface de 2 membros), DIP (contrato na biblioteca, implementação no serviço)
- **Datas** — `DateTime.Kind`, `SpecifyKind`, `DateTimeOffset`, RFC-822 com fuso numérico, `CultureInfo.InvariantCulture`
- **`decimal?`** para valor que só existe em parte dos itens · Newtonsoft.Json · NLog

## Estrutura

```
ColetaUOL/
├─ TVPlayerAPI/                              → biblioteca (AnyCPU, sem CefSharp)
│  ├─ UOL/
│  │  ├─ UOLScraper.cs                       → listagem → matérias → XML de encaixe (arquivo completo)
│  │  ├─ Classes/UOLSecao.cs                 → as 8 seções como dado (OCP)
│  │  ├─ Fontes/
│  │  │  ├─ IFonteHtml.cs                    → o contrato
│  │  │  ├─ FonteHttp.cs                     → HttpClient + renovação de conexão
│  │  │  └─ FontesHtml.cs                    → escolhe a fonte; recebe o navegador de fora
│  │  └─ Cotacoes/
│  │     ├─ Interfaces/IFonteCotacoesUOL.cs
│  │     ├─ Clients/CotacoesUOLApiClient.cs  → transporte
│  │     ├─ Parsers/ResumoCotacaoParser.cs   → JSON → objeto; fuso de Brasília → UTC
│  │     ├─ Classes/                         → IndicadorCotacao (ordem da tela), TipoIndicador, CotacaoUOL, LinhaCotacao
│  │     ├─ Services/UOLCotacoesService.cs   → regra: tudo ou nada, 8 linhas na ordem
│  │     └─ Saida/EscritorCotacoesXml.cs     → XML rss/channel/item
│  └─ Helpers/DateTimeHelper.cs              → pubDate em UTC com +0000
└─ TI.TVPlayer.Service/                      → serviço (x86, já tinha o CefSharp)
   └─ Util/
      ├─ NavegadorCef.cs                     → IFonteHtml via Chromium + FiltroDeRecursos
      └─ CefCompartilhado.cs                 → Cef.Initialize único + semáforo UOL/Loteria
```

> 📄 São **trechos representativos** (17 arquivos, ~1.200 linhas) de um projeto maior: o `RSSClient` (merge, MD5, download e redimensionamento da imagem, gravação da pasta), o `RSSRecover` que desvia cada tipo para sua coleta, o `HttpDedicado` e o `WebHelper` não foram incluídos — por isso o código não compila sozinho.

> 🔒 Nenhuma credencial neste diretório: os únicos endereços no código são as páginas e a API **públicas** do UOL que o próprio site consome.
