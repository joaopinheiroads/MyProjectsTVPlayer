using Newtonsoft.Json.Linq;
using NLog;
using SerializableObjects.PluginsTVPlayer;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using TVPlayerAPI.Helpers;
using TVPlayerAPI.UOL.Classes;
using TVPlayerAPI.UOL.Fontes;

namespace TVPlayerAPI.UOL
{
    internal static class UOLScraper
    {
        private const int MAX_ITENS = 15;
        private const int PAUSA_ENTRE_MATERIAS_MS = 200;
        private const int MAX_CANDIDATAS_IMAGEM = 6;

        private static readonly string[] ESCADA_RESOLUCAO = { "1920x1080", "1200x675" };

        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private static readonly Regex RegexLdJson =
            new Regex("<script[^>]*type=\"application/ld\\+json\"[^>]*>(.*?)</script>",
                      RegexOptions.Singleline | RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex RegexMeta =
            new Regex("<meta[^>]+(?:property|name)=\"(?<k>og:title|og:image|og:description|article:published_time)\"[^>]+content=\"(?<v>[^\"]*)\"",
                      RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex RegexCredito =
            new Regex("Imagem:\\s*(?<c>[^<\"]{2,120}?)\\s*(?:<|\")",
                      RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex RegexDimensaoImagem =
            new Regex(@"^(?<base>.+?)_(?<w>\d{2,4})x(?<h>\d{2,4})(?<ext>\.[a-z]{3,4})(?:\?.*)?$",
                      RegexOptions.IgnoreCase | RegexOptions.Compiled);

        internal static async Task<KeyValuePair<string, RssRecoverType>> RecuperarAsync(RSSType tipo)
        {
            UOLSecao secao = UOLSecao.Por(tipo);

            if (secao == null)
                return new KeyValuePair<string, RssRecoverType>(string.Empty, RssRecoverType.NaoExiste);

            IFonteHtml fonte = FontesHtml.Para(secao);

            try
            {
                logger.Info("UOL: " + secao.CategoriaPadrao + " pela fonte \"" + fonte.Nome + "\".");

                List<string> links = await ObterLinksAsync(secao, fonte).ConfigureAwait(false);

                if (links.Count == 0)
                {
                    logger.Info("UOL: " + secao.CategoriaPadrao + " sem materias nesta rodada (" +
                                secao.HostEsperado + " recusou a listagem); a pasta mantem o conteudo anterior.");
                    return new KeyValuePair<string, RssRecoverType>(MontarXml(new List<ItemUOL>()), RssRecoverType.SemConteudo);
                }

                List<string> alvo = links.Take(MAX_ITENS).ToList();
                var itens = new List<ItemUOL>();

                foreach (string link in alvo)
                {
                    ItemUOL item = await ObterMateriaAsync(link, secao, fonte).ConfigureAwait(false);

                    if (item != null)
                        itens.Add(item);

                    await Task.Delay(PAUSA_ENTRE_MATERIAS_MS).ConfigureAwait(false);
                }

                if (itens.Count == 0)
                {
                    logger.Warn("UOL: " + links.Count + " links em " + secao.CategoriaPadrao + ", nenhum rendeu materia valida.");
                    return new KeyValuePair<string, RssRecoverType>(MontarXml(new List<ItemUOL>()), RssRecoverType.SemConteudo);
                }

                if (itens.Count < alvo.Count)
                    logger.Info("UOL: " + secao.CategoriaPadrao + " coletou " + itens.Count + " de " + alvo.Count + " materias; o merge completa o resto.");

                return new KeyValuePair<string, RssRecoverType>(MontarXml(itens), RssRecoverType.Sucesso);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "UOL: falha ao coletar " + secao.CategoriaPadrao + " em " + string.Join(", ", secao.UrlsListagem) + ".");
                return new KeyValuePair<string, RssRecoverType>(string.Empty, RssRecoverType.Erro);
            }
        }

        private static async Task<List<string>> ObterLinksAsync(UOLSecao secao, IFonteHtml fonte)
        {
            var links = new List<string>();

            foreach (string urlListagem in secao.UrlsListagem)
            {
                string html = await fonte.ObterHtmlAsync(urlListagem).ConfigureAwait(false);

                if (string.IsNullOrEmpty(html))
                {
                    logger.Info("UOL: listagem recusada pela fonte \"" + fonte.Nome + "\": " + urlListagem);
                    continue;
                }

                foreach (JToken lista in LerLdJson(html).Where(t => (string)t["@type"] == "ItemList"))
                {
                    JToken elementos = lista["itemListElement"];

                    if (elementos == null)
                        continue;

                    foreach (JToken item in Achatar(elementos))
                    {
                        string url = (string)item["url"];

                        if (!string.IsNullOrEmpty(url))
                            links.Add(url);
                    }
                }
            }

            int total = links.Count;

            links = links
                .Where(u => EhDoHost(u, secao.HostEsperado))
                .Distinct()
                .ToList();

            if (total > links.Count)
                logger.Debug("UOL: " + secao.CategoriaPadrao + " descartou " + (total - links.Count) + " de " + total + " links de outros sites do grupo.");

            return links;
        }

        private static bool EhDoHost(string url, string host)
        {
            Uri parsed;
            return Uri.TryCreate(url, UriKind.Absolute, out parsed)
                && parsed.Host.Equals(host, StringComparison.OrdinalIgnoreCase);
        }

        private static async Task<ItemUOL> ObterMateriaAsync(string url, UOLSecao secao, IFonteHtml fonte)
        {
            string html = await fonte.ObterHtmlAsync(url).ConfigureAwait(false);

            if (string.IsNullOrEmpty(html))
            {
                logger.Info("UOL: materia recusada pela fonte \"" + fonte.Nome + "\": " + url);
                return null;
            }

            Dictionary<string, string> metas = LerMetas(html);
            JToken artigo = LerLdJson(html).FirstOrDefault(t => ((string)t["@type"] ?? string.Empty).Contains("Article"));

            string manchete = Decodificar(Valor(metas, "og:title") ?? (artigo == null ? null : (string)artigo["headline"]));
            string imagem = Valor(metas, "og:image") ?? PrimeiraImagem(artigo);

            if (string.IsNullOrWhiteSpace(manchete) || string.IsNullOrWhiteSpace(imagem))
            {
                logger.Warn("UOL: materia sem manchete ou sem imagem, descartada: " + url);
                return null;
            }

            if (EhImagemGenerica(imagem))
            {
                logger.Warn("UOL: materia sem foto propria (imagem generica), descartada: " + url);
                return null;
            }

            string imagemFinal = await MaiorVarianteQueRespondeAsync(html, imagem).ConfigureAwait(false);

            if (imagemFinal == null)
            {
                logger.Info("UOL: nenhuma variante da imagem respondeu, materia descartada: " + url);
                return null;
            }

            return new ItemUOL
            {
                Categoria = secao.CategoriaPadrao,
                Manchete = manchete,
                Imagem = imagemFinal,
                Credito = LerCredito(html),
                Publicacao = LerData(metas, artigo)
            };
        }

        private static bool EhImagemGenerica(string url)
        {
            return !RegexDimensaoImagem.IsMatch(url);
        }

        private static async Task<string> MaiorVarianteQueRespondeAsync(string html, string ogImage)
        {
            Match origem = RegexDimensaoImagem.Match(ogImage);

            if (!origem.Success)
                return null;

            string raiz = origem.Groups["base"].Value;
            string ext = origem.Groups["ext"].Value;

            var candidatas = new List<KeyValuePair<int, string>>();

            foreach (string degrau in ESCADA_RESOLUCAO)
                candidatas.Add(new KeyValuePair<int, string>(int.Parse(degrau.Split('x')[0]),
                                                             raiz + "_" + degrau + ext));

            var citadas = new Regex(Regex.Escape(raiz) + @"_(?<w>\d{2,4})x(?<h>\d{2,4})" +
                                    Regex.Escape(ext) + @"(?!\.webp)",
                                    RegexOptions.IgnoreCase);

            foreach (Match m in citadas.Matches(html))
            {
                int largura = int.Parse(m.Groups["w"].Value);
                int altura = int.Parse(m.Groups["h"].Value);

                if (largura > altura)
                    candidatas.Add(new KeyValuePair<int, string>(largura, m.Value));
            }

            candidatas.Add(new KeyValuePair<int, string>(int.Parse(origem.Groups["w"].Value), ogImage));

            foreach (var candidata in candidatas
                                        .GroupBy(c => c.Value)
                                        .Select(g => g.First())
                                        .OrderByDescending(c => c.Key)
                                        .Take(MAX_CANDIDATAS_IMAGEM))
            {
                if (await WebHelper.ExisteAsync(candidata.Value).ConfigureAwait(false))
                    return candidata.Value;
            }

            return null;
        }

        private static string LerCredito(string html)
        {
            Match m = RegexCredito.Match(html);
            return m.Success ? Decodificar(m.Groups["c"].Value) : string.Empty;
        }

        private static string LerData(Dictionary<string, string> metas, JToken artigo)
        {
            DateTime publicacao = DataDoLdJson(artigo?["datePublished"])
                               ?? DataDoTexto(Valor(metas, "article:published_time"))
                               ?? DateTime.UtcNow;

            return DateTimeHelper.PubDateRssEmUtc(publicacao);
        }

        private static DateTime? DataDoLdJson(JToken valor)
        {
            if (valor is JValue bruto && bruto.Value is DateTime jaConvertida)
                return jaConvertida;

            return valor?.Type == JTokenType.String ? DataDoTexto((string)valor) : null;
        }

        private static DateTime? DataDoTexto(string texto)
        {
            if (string.IsNullOrEmpty(texto))
                return null;

            return DateTime.TryParse(texto, CultureInfo.InvariantCulture,
                                     DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeLocal, out DateTime data)
                ? data
                : (DateTime?)null;
        }

        private static string MontarXml(List<ItemUOL> itens)
        {
            XNamespace xsi = "http://www.w3.org/2001/XMLSchema-instance";
            XNamespace xsd = "http://www.w3.org/2001/XMLSchema";

            var canal = new XElement("channel",
                itens.Select(i =>
                    new XElement("item",
                        new XElement("title", i.Categoria),
                        new XElement("description", i.Manchete),
                        new XElement("enclosure",
                            new XAttribute("type", "image"),
                            new XAttribute("url", i.Imagem)),
                        new XElement("author", i.Credito),
                        new XElement("pubDate", i.Publicacao))));

            return new XDocument(
                new XElement("rss",
                    new XAttribute(XNamespace.Xmlns + "xsi", xsi),
                    new XAttribute(XNamespace.Xmlns + "xsd", xsd),
                    new XAttribute("version", "2.0"),
                    canal)).ToString();
        }

        private static IEnumerable<JToken> LerLdJson(string html)
        {
            foreach (Match m in RegexLdJson.Matches(html))
            {
                JToken token = null;

                try
                {
                    token = JToken.Parse(m.Groups[1].Value);
                }
                catch
                {
                    continue;
                }

                foreach (JToken achatado in Achatar(token))
                    yield return achatado;
            }
        }

        private static IEnumerable<JToken> Achatar(JToken token)
        {
            if (token is JArray)
            {
                foreach (JToken filho in token)
                {
                    foreach (JToken neto in Achatar(filho))
                        yield return neto;
                }
            }
            else if (token != null)
            {
                yield return token;
            }
        }

        private static Dictionary<string, string> LerMetas(string html)
        {
            var metas = new Dictionary<string, string>();

            foreach (Match m in RegexMeta.Matches(html))
            {
                string chave = m.Groups["k"].Value.ToLowerInvariant();

                if (!metas.ContainsKey(chave))
                    metas[chave] = m.Groups["v"].Value;
            }

            return metas;
        }

        private static string Valor(Dictionary<string, string> metas, string chave)
        {
            string v;
            return metas.TryGetValue(chave, out v) && !string.IsNullOrWhiteSpace(v) ? v : null;
        }

        private static string PrimeiraImagem(JToken artigo)
        {
            JToken imagem = artigo == null ? null : artigo["image"];

            if (imagem == null)
                return null;

            if (imagem is JArray)
                return (string)imagem.FirstOrDefault();

            return (string)(imagem["url"] ?? imagem);
        }

        private static string Decodificar(string texto)
        {
            return string.IsNullOrEmpty(texto) ? texto : WebUtility.HtmlDecode(texto).Trim();
        }

        private class ItemUOL
        {
            internal string Categoria { get; set; }
            internal string Manchete { get; set; }
            internal string Imagem { get; set; }
            internal string Credito { get; set; }
            internal string Publicacao { get; set; }
        }
    }
}
