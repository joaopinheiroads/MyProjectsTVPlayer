using Newtonsoft.Json.Linq;
using NLog;
using System;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using TVPlayerAPI.GazetaDoPovo.Classes;
using TVPlayerAPI.Helpers;

namespace TVPlayerAPI.GazetaDoPovo.Parsers
{
    internal static class MateriaGazetaParser
    {
        private const int LARGURA_MAXIMA_DO_ORIGINAL = 2560;
        private const int LARGURA_MAXIMA_DA_VARIANTE = 1920;

        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        internal static MateriaGazeta Ler(string html)
        {
            JObject artigo = LeitorLdJson.PrimeiroArtigo(html);
            string manchete = TituloEditorial(html) ?? TextoLimpo(artigo?["headline"]);
            string imagem = ImagemPreferida(html, artigo?["image"]);

            if (manchete == null || imagem == null)
                return null;

            return new MateriaGazeta(manchete,
                                     TextoLimpo(artigo?["description"]) ?? string.Empty,
                                     imagem,
                                     DataDePublicacao(artigo?["datePublished"]));
        }

        private static string TituloEditorial(string html)
        {
            JObject trilha = LeitorLdJson.PrimeiroDoTipo(html, "BreadcrumbList");

            if (!(trilha?["itemListElement"] is JArray passos) || passos.Count == 0)
                return null;

            return TextoLimpo((passos.Last as JObject)?["name"]);
        }

        private static string ImagemPreferida(string html, JToken imagem)
        {
            JToken principal = imagem is JArray varias ? varias.FirstOrDefault() : imagem;
            JObject descrita = principal as JObject;
            string original = descrita == null ? TextoLimpo(principal) : TextoLimpo(descrita["url"]);

            if (original == null)
                return null;

            int largura = 0;
            bool larguraConhecida = descrita != null && int.TryParse(descrita["width"]?.ToString(), out largura);

            if (larguraConhecida && largura <= LARGURA_MAXIMA_DO_ORIGINAL)
                return original;

            string variante = MaiorVarianteCitada(html, original);

            if (variante != null)
                logger.Debug("Gazeta do Povo: imagem de " + (larguraConhecida ? largura + "px" : "largura desconhecida") + " trocada pela variante " + variante);

            return variante ?? original;
        }

        private static string MaiorVarianteCitada(string html, string original)
        {
            int ponto = original.LastIndexOf('.');

            if (ponto <= original.LastIndexOf('/'))
                return null;

            var citacao = new Regex(Regex.Escape(original.Substring(0, ponto))
                                    + @"-(?<largura>\d{2,4})x(?<altura>\d{2,4})"
                                    + Regex.Escape(original.Substring(ponto)));

            return citacao.Matches(html)
                          .Cast<Match>()
                          .Select(m => new
                          {
                              Url = m.Value,
                              Largura = int.Parse(m.Groups["largura"].Value),
                              Altura = int.Parse(m.Groups["altura"].Value)
                          })
                          .Where(variante => variante.Largura <= LARGURA_MAXIMA_DA_VARIANTE && variante.Largura > variante.Altura)
                          .OrderByDescending(variante => variante.Largura)
                          .Select(variante => variante.Url)
                          .FirstOrDefault();
        }

        private static DateTime DataDePublicacao(JToken valor)
        {
            if (valor == null)
                return DateTime.MinValue;

            if (valor.Type == JTokenType.Date)
                return valor.ToObject<DateTime>().ToUniversalTime();

            return DateTime.TryParse(valor.ToString(), CultureInfo.InvariantCulture,
                                     DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out DateTime data)
                ? data
                : DateTime.MinValue;
        }

        private static string TextoLimpo(JToken valor)
        {
            if (valor == null || valor.Type != JTokenType.String)
                return null;

            string texto = WebUtility.HtmlDecode(valor.ToString()).Trim();

            return texto.Length == 0 ? null : texto;
        }
    }
}
