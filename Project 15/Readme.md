# Projeto 15 — Integrações confiáveis: fim das falhas silenciosas

Seis correções no pipeline que alimenta os plugins de conteúdo do TV Player (notícias, futebol, loterias) — um serviço Windows em .NET Framework que coleta dados de dezenas de fontes externas e grava os arquivos que **centenas de terminais** exibem. O resultado: **uma credencial que vazava para todos os provedores foi isolada, o plugin do Tecmundo (82 terminais) voltou a atualizar depois de três meses parado, e uma foto ruim deixou de derrubar o plugin inteiro.**

> **O tema comum é a falha que não avisa.** Um feed que responde `200` com conteúdo velho, um plugin que desiste em silêncio, uma chave enviada a quem não devia. Nenhum desses defeitos gerava alerta — a tela do cliente simplesmente parava no tempo.
>
> **Muda só a coleta; a entrega é intocável.** O player instalado nos terminais não pode ser atualizado, então o formato de saída (`contents.json`, `contents.xml`, `rss.xml`, `data.xml`) é contrato. Todas as correções abaixo preservam esse contrato byte a byte — em um dos casos, o arquivo gerado saiu com **MD5 idêntico** ao de produção.

## O problema

O serviço cresceu por acréscimo, integração após integração, e acumulou seis falhas que tinham a mesma natureza: **ninguém ficava sabendo**.

- **Credencial vazando por estado global** — o cliente do football-data escrevia a chave `X-Auth-Token` no `HttpClient` **estático e compartilhado** do sistema inteiro. A partir do primeiro ciclo, a chave ia junto em toda requisição de todo provedor: UOL, Canaltech, G1, iCarros, Jovem Pan, rss.app. Além disso, clientes HTTP recriados a cada ciclo levavam ao risco de **esgotamento de sockets**.
- **Log cego** — só o UOL tinha arquivo de log próprio. Canaltech, Jovem Pan, Football, Instagram e o merge do RSS caíam todos num `tvplayer.log` genérico, e só de `Info` para cima. O `RSSClient`, que atende 42 plugins, **não tinha logger nenhum**.
- **Tudo ou nada nas imagens** — no `RSSClient.SaveOnDisk`, uma única foto que não baixava (403) ou não abria (veio em gzip) lançava exceção e o plugin inteiro desistia da rodada. A Gazeta do Povo – Economia (34 terminais) parava sempre que uma foto vinha comprimida; o Campo Grande News nunca gravou.
- **Regra de imagem estreita demais** — o Tecmundo publica `<media:content url="...jpg">` sem `medium` nem `type`. A regra antiga exigia um dos dois, concluía que nenhuma notícia tinha foto e lançava `"Esse feed não possui imagens."`. **82 terminais com as mesmas notícias desde 03/06.**
- **Configuração dentro do código** — a chave da API e as datas de cada fase dos campeonatos (Brasileirão e Champions League) eram literais no `FootballDataClient`. Mudar uma data exigia recompilar e subir DLL.
- **Trabalho inútil no navegador** — o Lotogol saiu de linha, mas o laço de coleta percorria **todo o enum** `ELoteria` e abria o navegador CEF (compartilhado com o UOL) de hora em hora para buscá-lo na Caixa.

## A solução

| Correção | Onde | O que muda |
|---|---|---|
| **1. `HttpDedicado`** | `Helpers/HttpDedicado.cs` | Um `HttpClient` **por integração**, guardado em `static readonly` (vive o processo inteiro, sem esgotar sockets). Cabeçalhos exclusivos — como o `X-Auth-Token` — ficam presos à instância que só aquela integração usa. Descompressão gzip/deflate ligada só para quem opta. |
| **2. Log por serviço** | `NLog.config`, `HttpDedicado`, `RSSClient` | Um arquivo por fonte (Canaltech, Jovem Pan, Football, Tecmundo, Gazeta do Povo, G1…). Regras em par: o serviço escreve do `Debug` para cima no próprio arquivo; um "portão" `maxlevel="Info" final="true"` segura o ruído; `Warn` e `Error` seguem e aparecem **também** no log geral. O nome do logger é calculado no código (`TVPlayerAPI.<Fonte>.Http`, `TVPlayerAPI.<Fonte>.RSS`) — é ele que decide o roteamento. |
| **3. Descarte por notícia** | `RSSClient.SaveOnDisk` | `throw` virou `continue`: a falha passa a ser tratada no escopo da **notícia**, não do feed. `ImagemLegivel` converte exceção em `bool` antes da conversão. Se **todas** caírem, devolve `SemConteudo` sem tocar na pasta (em vez de gravar um `contents.json` vazio). Download com prazo (`CancellationTokenSource`, 20 s) e desistência após 2 fotos que não baixam, para uma origem lenta não travar a fila. |
| **4. Regra `media:content`** | `RSSCustomClient.IsImageMedia` | Três saídas: se declara imagem, aceita; se declara outra mídia (vídeo), recusa; se não declara nada, decide pela **extensão do caminho** (`Uri.AbsolutePath`, ignorando query string). A regra frouxa ("aceita qualquer `url`") consertaria 1 plugin arriscando os outros 41. |
| **5. Configuração fora do código** | `FootballDataClient`, `App.config` | Chave e datas no `appSettings` do **executável** (não da DLL). Datas lidas com `TryParseExact` + `InvariantCulture`; valor ausente vira `Warn` no log, não exceção. As faixas de cada fase são **derivadas** dos limites: 13 chaves em vez de 25 literais. |
| **6. `LoteriasColetadas`** | `Service/Util/LoteriasColetadas.cs` | Lista de **exclusão** sobre o enum. O enum continua sendo contrato (é o nome da chave no `contents.json`); o que se coleta hoje é decisão operacional e entra como **dado** (OCP). Loteria nova no enum é coletada sozinha. |

### Princípios que guiaram as decisões

- **Tratar a falha no escopo certo** — a foto 403 é problema da notícia, não do feed; a chave do Football é problema do Football, não do sistema.
- **Falhar alto, nunca calado** — toda desistência agora deixa uma linha com o nome do plugin e o ID (`Gazeta do Povo - Economia [2025]`), em vez de um `Console.WriteLine` que ninguém lê.
- **Não afrouxar a regra inteira** — acrescentar o caso que faltava e manter as recusas que protegem os outros plugins.
- **Mudar o mínimo** — o `RSSCustomClient` não migrou para o `HttpDedicado` de propósito: o ganho não pagava o risco de mexer no provedor que atende 41 plugins de cliente.

## Resultado

| | Antes | Depois |
|---|---|---|
| Chave do football-data | enviada a **todos** os provedores | só nas requisições do Football (nenhum cabeçalho escrito no cliente compartilhado) |
| Chave no código-fonte | literal compilada na DLL | `App.config` do serviço |
| Jovem Pan (6 pastas) | paradas há **3 meses** | 15 itens cada, imagem em todas |
| Arquivos de log dedicados | 1 (UOL) | **13** + o geral |
| Logs do `RSSClient` (42 plugins) | nenhum | resultado de cada rodada, por plugin |
| Tecmundo (82 terminais) | parado desde 03/06 | **15/15 notícias com imagem** |
| Gazeta do Povo – Economia (34 terminais) | `ErroAoSalvar`, rodada perdida | 14 notícias gravadas, 1 descartada |
| Feed com 100% de fotos quebradas | gravaria tela vazia | `SemConteudo`, mantém o conteúdo anterior |
| Pior caso de um plugin com fotos penduradas | fila parada 11 min (até 15 min por foto) | **2 min** e a fila segue |
| Trocar uma data de campeonato | recompilar e subir DLL | editar config e reiniciar |
| Navegações no CEF por ciclo de loterias | 12 | 11 (Lotogol mantido no JSON, intacto) |

### Como foi provado

- **Harness 32 bits** carregando a DLL real, exatamente como o serviço faz, contra feeds reais: Tecmundo passou de exceção para sucesso, e os **6 plugins de controle saíram idênticos** antes e depois.
- **Servidor local** com casos forçados (foto boa, foto em gzip, foto 403, foto que nunca responde) — o registro do próprio servidor mostrou que as fotos após o limite **nunca foram pedidas**.
- **Equivalência de configuração:** a escada de datas nova contra 11 datas cobrindo todos os ramos gerou as mesmas URLs; o arquivo de saída do Football saiu com **MD5 idêntico** ao de produção. Em produção, o ciclo levou 126 s contra 125 s medidos em teste.
- **Loterias:** depois do deploy, as outras 11 loterias avançaram de concurso e o Lotogol ficou no mesmo, na mesma posição do JSON — os **162 agendamentos ativos** continuaram lendo sem mudança.

## Tecnologias e conceitos

- **C# / .NET Framework** — serviço Windows (x86) + biblioteca de integrações
- **`HttpClient`** — ciclo de vida, socket exhaustion, `HttpClientHandler`, `AutomaticDecompression`
- **NLog** — roteamento por nome de logger, regras com `final`, layout renderers, `GetLogger` × `GetCurrentClassLogger`
- **`CancellationToken` / `CancellationTokenSource`** — prazo por download sem reconfigurar o cliente compartilhado
- **XML / XPath / `XmlNamespaceManager`** — leitura de Media RSS (`media:content`, `media:group`)
- **`ConfigurationManager`** — `appSettings` do executável, ordem de inicializadores estáticos, `TryParseExact` + `InvariantCulture`
- **LINQ** — `Where`, `Contains`, `Cast`, execução adiada
- **SOLID** — SRP (um cliente por integração), OCP (lista de dado em vez de `switch`)
- **Compatibilidade de contrato** — saída congelada para um player que não pode ser atualizado

## Estrutura

```
IntegracoesConfiaveis/
├─ api/
│  ├─ Helpers/HttpDedicado.cs                 → um HttpClient por integração, logger com nome da fonte
│  ├─ RSS/Client/RSSClient.cs                 → trechos: logger por fonte, merge instrumentado,
│  │                                            SaveOnDisk com descarte por notícia, ImagemLegivel
│  ├─ RSSCustom/RSSCustomClient.cs            → IsImageMedia / HasImageExtension (regra media:content)
│  └─ FootballData/Client/FootballDataClient.cs → trechos: chave e datas lidas do config, Janela, LerLimite
└─ Service/
   ├─ NLog.config                             → recorte: alvos e regras em par por serviço
   ├─ App.config                              → recorte: appSettings com valores em placeholder
   └─ Util/LoteriasColetadas.cs               → lista de exclusão sobre o enum ELoteria
```

> 📄 `RSSClient.cs` e `FootballDataClient.cs` são **recortes**: só os campos e métodos tocados pelas correções, sem o restante dos arquivos. `NLog.config` traz uma parte dos alvos (o real tem 13 serviços dedicados).

> 🔒 Nenhuma credencial neste diretório: a chave do football-data e o token do Apify aparecem como `<REDACTED>`, e as datas dos campeonatos como `yyyy-MM-dd`.
