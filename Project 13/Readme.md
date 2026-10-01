# Projeto 13 — Plugins de notícia sem RSS (scraping em camadas, contrato congelado)

Migração de **7 fontes de notícia** (G1, iG, iCarros, Gazeta do Povo, Infomoney, TVFoco e Tecmundo) que alimentavam as telas dos terminais a partir de feeds RSS para a **leitura do próprio site**: a API que a página consome, o sitemap, o `ld+json` e as tags `og:`. Tudo em camadas SOLID e **sem mudar um byte do que o player recebe**. Resultado: **pastas paradas desde 2019, junho, julho e agosto voltaram a atualizar**, e o TVFoco passou de **0 para 15 de 15 notícias com foto**.

> **A coleta muda, a entrega fica congelada.** O player instalado nos terminais não pode ser atualizado, então o formato de saída é contrato: os mesmos arquivos (`contents.json`, `contents.xml`, `rss.xml`, `data.xml`), o mesmo XML intermediário `rss/channel/item`, a mesma gravação via `RSSClient.SaveOnDisk` e os mesmos IDs. Cada fonte foi validada comparando a saída nova com a de produção, campo a campo.
>
> *(O Canaltech foi o primeiro plugin migrado assim e serviu de modelo para os demais.)*

## O problema

Feed RSS falha **calado**: responde `200` com conteúdo velho, e ninguém percebe que a tela parou no tempo. Na auditoria das fontes apareceu de tudo:

- **iG** — vinha de um **FTP** que autenticava normalmente, mas cujos arquivos eram de **novembro de 2019**. A migração também tirou do código as credenciais desse acesso.
- **G1** — três seções (Ciência e Saúde, Concursos, Natureza) estavam paradas desde junho/agosto porque o g1 **renomeou as seções** e os feeds antigos simplesmente pararam.
- **iCarros** — o XML "midiaindoor" trazia só **5 matérias novas** e completava a lista com matérias de **2019 e 2025**.
- **Tecmundo** — o site foi incorporado ao Estadão e responde `301`; o feed antigo seguia `200` com a última notícia de 07/09.
- **TVFoco** — parado desde 29/07: o feed passou a redirecionar com `308`, que o `HttpClient` do .NET Framework não segue. E mesmo quando lia, as notícias saíam **sem foto**, porque o XSL procurava `.jpg` e o site só serve `.webp`.
- **Infomoney** — o feed colava *"The post ... appeared first on InfoMoney."* no fim de **14 de 15 resumos** que iam para a tela.
- **Tecmundo e Gazeta do Povo** — os primeiros scrapers eram **uma classe estática que fazia tudo** (o do Tecmundo com 281 linhas): baixar, interpretar, decidir e montar o XML.

## A solução

Cada fonte virou uma pasta com as mesmas camadas, uma responsabilidade por arquivo:

| Camada | Responsabilidade | Exemplo (G1) |
|--------|------------------|--------------|
| **Classes** (model) | O dado, imutável, sem IO. Seções/sites como **lista de dados**, não `switch`. | `G1Secao`, `NoticiaG1`, `MateriaG1` |
| **Interfaces** | O contrato da fonte — pequeno, uma coisa só (ISP). | `IFonteNoticiaG1`, `IFonteMateriaG1` |
| **Clients** | Só o transporte: pedir e devolver texto/JSON. | `G1FalkorClient`, `G1MateriaClient` |
| **ResponseWrappers** | A forma do JSON de terceiros — o único lugar que conhece os nomes deles. | `FalkorResponse` |
| **Parsers** | Só interpretar: HTML/JSON/XML → objeto nosso, sem rede. | `FalkorPostMapper`, `G1UrlDoPlugin` |
| **Services** | A regra do plugin: o que coletar, em que ordem, o que descartar, quantos entregar. | `G1Service` |
| **Saida** | Escreve o XML intermediário congelado `rss/channel/item`. | `EscritorFeedG1Xml` |

Dois helpers são compartilhados entre as fontes: **`LeitorLdJson`** (lê os blocos `application/ld+json` de qualquer página, achatando `@graph` e listas) e **`LeitorOpenGraph`** (lê as tags `og:`).

O service recebe as fontes **pelo construtor, por interface** (DIP); um único ponto estático (`RecuperarAsync`) escolhe a implementação concreta. O `RSSRecover` só pergunta `Atende(url)` e repassa — a URL cadastrada no banco vira apenas um identificador, então **nenhum SQL e nenhuma mudança de cadastro** foram necessários.

### Uma fonte por estratégia

| Fonte | Antes | Depois |
|-------|-------|--------|
| **G1** | RSS por seção (3 seções mortas) | API **Falkor** que a própria página da seção consome + `og:` da matéria para foto em tamanho original |
| **iG** | FTP com arquivos de 2019 | API **`contentnews`** do site; o FTP e a credencial saíram do código |
| **iCarros** | XML midiaindoor + XSLT | **Arquivo de notícias** do site (HTML cronológico, paginado) + autor da página da matéria |
| **Gazeta do Povo** | Feed RSS por seção | **`ld+json`** da listagem (`ItemList`) e da matéria (título, foto, data e `BreadcrumbList`) |
| **Infomoney** | `/feed/` do WordPress | **API REST do WordPress** (`/wp-json/wp/v2/posts`) — genérica: site novo entra como uma linha de dado |
| **TVFoco** | Feed com `308` + XSL | **`sitemap-news.xml`** + `ld+json` da matéria (foto `.webp` convertida para `.jpg` no pipeline) |
| **Tecmundo** | 1 classe de 281 linhas | **Sitemap do dia** do Estadão + `ld+json`, quebrado em camadas |

### Decisões que só apareceram nos testes

- **Deduplicar antes de cortar.** Cortar em 15 antes de remover repetidos deixava seções com 9 itens.
- **Pedir 25 para entregar 15** (Infomoney). O merge do `RSSClient` só mistura com a rodada anterior quando chegam menos de 15 itens, e a chave do merge inclui a descrição: com 14, a mesma notícia apareceria duas vezes na tela (versão com e sem o rodapé).
- **Notícia sem resumo não sai** (G1). O `RSSClient` omite `<description>` vazio, e o player nunca tinha recebido um item assim.
- **Extensão da foto opcional** (iCarros). O servidor entrega `.webp` ou `.jpg` conforme o `Accept`; a primeira versão exigia `.jpg` e voltava vazia.
- **Imitar o XSLT elemento por elemento** (iCarros, TVFoco), inclusive a ordem dos namespaces e o `channel` sem `title`, porque não havia linha de base nova para comparar.

## Resultado

| | Antes | Depois |
|---|---|---|
| Fontes lendo feed RSS/FTP/XSLT | **7** | **0** |
| iG | arquivos de **nov/2019** via FTP, com credencial no código | API do site, **15 itens por seção**, 60/60 imagens em 1200x675 |
| G1 | 3 de 12 seções paradas | **12/12** com 15 itens, nenhum sem descrição, nenhum título repetido |
| iCarros | 5 matérias novas + 10 de 2019/2025 | **15 matérias atuais**, mesmas fotos 1024x860 |
| TVFoco | parado desde 29/07, **0 de 15** com foto | **15 de 15** com foto, mesmos títulos, autores e horários |
| Infomoney | 14 de 15 resumos com "appeared first on" | **0 de 15** |
| Gazeta do Povo | fotos de 960x540 do feed | fotos de **970 a 1920 px** do site |
| Tecmundo | 1 classe, 281 linhas | 8 arquivos em camadas, **XML idêntico** ao anterior |
| Formato entregue ao player | — | **inalterado** (mesmos 4 arquivos, campos, ordem, `CDATA`, BOM) |

### Como cada migração foi provada

- **Harness 32 bits** carregando a DLL compilada e chamando o service por reflexão, contra o site real.
- **Lado a lado**: DLL de produção → DLL nova → DLL de produção de novo, comparando o XML (Gazeta e Tecmundo saíram **idênticos**, ignorando só os ticks do `author`).
- **Pipeline completo** sobre cópias das pastas de produção: estrutura dos 4 arquivos idêntica à de produção.
- **Em produção**: G1 (12 pastas regravadas no primeiro ciclo), iG, iCarros e Infomoney. Gazeta do Povo (camadas), Tecmundo (camadas) e TVFoco estão validados e aguardando deploy.

### Ganhos além do conteúdo novo

- **Segurança** — a remoção do FTP do iG tirou de dentro da DLL publicada uma credencial que qualquer pessoa com o arquivo conseguia ler.
- **Falha visível** — fonte vazia agora devolve `SemConteudo`, mantém a pasta anterior e deixa uma linha de `Warn` no log próprio de cada integração (NLog por namespace), em vez de entregar conteúdo velho em silêncio.
- **OCP de verdade** — no Infomoney, um site WordPress saiu e voltou no mesmo dia por decisão de produto; nos dois sentidos, a mudança foi **uma linha de dado**.
- **Substituível** — se a API de uma fonte morrer, entra outra implementação da interface (ex.: um scraper de HTML) sem tocar no service.

## Tecnologias e conceitos

- **C# / .NET Framework** · `async`/`await` com `ConfigureAwait(false)`
- **SOLID** — SRP (uma responsabilidade por arquivo), OCP (seções e sites como dado), ISP (interfaces de 1-2 membros), DIP (service depende de interface)
- **Web scraping sem RSS** — APIs internas dos sites (Falkor, contentnews, WP REST), **sitemaps** (sitemaps.org / Google News), **JSON-LD** (schema.org) e **Open Graph**
- **Newtonsoft.Json** (`JToken`, wrappers com `[JsonProperty]`) e **LINQ to XML** (`XDocument`, namespaces)
- **LINQ** — `GroupBy` para deduplicar, `OrderByDescending`/`Take` para a regra dos 15
- **Regex** com grupos nomeados para HTML e URLs
- **Objetos imutáveis** e **Factory Method** (construtor privado + `DoPlugin`/`Para`)
- **NLog** com um arquivo de log por integração
- **Contrato de saída congelado** — compatibilidade com cliente legado que não pode ser atualizado

## Estrutura

Trechos representativos — não é a base inteira. O **G1 está completo** por ser o exemplo-modelo das camadas; das outras fontes, só o **Service** e os **Parsers**.

```
PluginsSemRSS/
├─ G1/                                     → completo: o modelo das camadas
│  ├─ Classes/        G1Secao, NoticiaG1, MateriaG1
│  ├─ Interfaces/     IFonteNoticiaG1, IFonteMateriaG1
│  ├─ Clients/        G1FalkorClient (API Falkor), G1MateriaClient (og:)
│  ├─ ResponseWrappers/FalkorResponse
│  ├─ Parsers/        G1UrlDoPlugin, FalkorInstanciaParser, FalkorPostMapper
│  ├─ Services/       G1Service             → regra: dedup, ordena, completa, corta em 15
│  └─ Saida/          EscritorFeedG1Xml     → XML intermediário congelado
├─ IG/                Services/IGService · Parsers/NoticiaIGMapper
├─ ICarros/           Services/ICarrosService · Parsers/ArquivoICarrosParser, MateriaICarrosParser
├─ GazetaDoPovo/      Services/GazetaDoPovoService · Parsers/ListagemGazetaParser, MateriaGazetaParser
├─ WordPress/         Services/WordPressService · Parsers/PostWordPressMapper   → Infomoney
├─ TVFoco/            Services/TVFocoService · Parsers/SitemapNewsTVFocoParser, MateriaTVFocoParser
├─ Tecmundo/          Services/TecmundoService · Parsers/SitemapEstadaoParser, MateriaEstadaoParser
└─ Helpers/
   ├─ LeitorLdJson.cs                      → JSON-LD de qualquer página
   └─ LeitorOpenGraph.cs                   → tags og: de qualquer página
```

> 📄 Os arquivos referenciam tipos da base que não estão aqui (`HttpDedicado`, `RSSRecover`, `RSSClient`, os models e escritores das outras fontes), então o diretório **não compila sozinho** — é vitrine do desenho.

> 🔒 Nenhuma credencial neste diretório: as únicas URLs são as públicas dos sites de notícia. O FTP do iG, que tinha credencial no código, foi **removido** da base.
