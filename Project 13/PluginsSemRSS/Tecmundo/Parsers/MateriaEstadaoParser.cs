using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using TVPlayerAPI.Helpers;
using TVPlayerAPI.Tecmundo.Classes;

namespace TVPlayerAPI.Tecmundo.Parsers
{
    internal static class MateriaEstadaoParser
    {
        private const double PROPORCAO_16_POR_9 = 16d / 9d;
        private const double TOLERANCIA_DA_PROPORCAO = 0.05;

        internal static MateriaTecmundo Ler(string html)
        {
            JObject artigo = LeitorLdJson.PrimeiroArtigo(html);
            string manchete = TextoLimpo(artigo?["headline"]);
            string imagem = MaiorImagemEm16Por9(artigo?["image"]);

            if (manchete == null || imagem == null)
                return null;

            return new MateriaTecmundo(manchete,
                                       TextoLimpo(artigo["description"]) ?? string.Empty,
                                       imagem,
                                       DataDePublicacao(artigo["datePublished"]));
        }

        private static string MaiorImagemEm16Por9(JToken imagens)
        {
            IEnumerable<JToken> candidatas = imagens is JArray lista ? (IEnumerable<JToken>)lista : new[] { imagens };

            return candidatas
                .OfType<JObject>()
                .Select(imagem => new
                {
                    Url = TextoLimpo(imagem["url"]),
                    Largura = Inteiro(imagem["width"]),
                    Altura = Inteiro(imagem["height"])
                })
                .Where(imagem => imagem.Url != null
                                 && imagem.Altura > 0
                                 && Math.Abs((double)imagem.Largura / imagem.Altura - PROPORCAO_16_POR_9) <= TOLERANCIA_DA_PROPORCAO)
                .OrderByDescending(imagem => imagem.Largura)
                .Select(imagem => imagem.Url)
                .FirstOrDefault();
        }

        private static int Inteiro(JToken valor)
        {
            return int.TryParse(valor?.ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out int numero) ? numero : 0;
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
