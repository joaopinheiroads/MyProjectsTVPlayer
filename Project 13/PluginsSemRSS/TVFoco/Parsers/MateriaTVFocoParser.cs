using Newtonsoft.Json.Linq;
using System;
using System.Globalization;
using System.Linq;
using System.Net;
using TVPlayerAPI.Helpers;
using TVPlayerAPI.TVFoco.Classes;

namespace TVPlayerAPI.TVFoco.Parsers
{
    internal static class MateriaTVFocoParser
    {
        internal static MateriaTVFoco Ler(string html)
        {
            JObject artigo = LeitorLdJson.PrimeiroArtigo(html);
            string titulo = TextoLimpo(artigo?["headline"]);
            DateTime? publicacao = DataDePublicacao(artigo?["datePublished"]);

            if (titulo == null || publicacao == null)
                return null;

            return new MateriaTVFoco(titulo,
                                     NomeDoAutor(artigo["author"]) ?? string.Empty,
                                     EnderecoDaImagem(artigo["image"]),
                                     publicacao.Value);
        }

        private static string NomeDoAutor(JToken autor)
        {
            JToken primeiro = autor is JArray varios ? varios.FirstOrDefault() : autor;

            return primeiro is JObject pessoa ? TextoLimpo(pessoa["name"]) : TextoLimpo(primeiro);
        }

        private static string EnderecoDaImagem(JToken imagem)
        {
            JToken primeira = imagem is JArray varias ? varias.FirstOrDefault() : imagem;
            string endereco = primeira is JObject descrita ? TextoLimpo(descrita["url"]) : TextoLimpo(primeira);

            return Uri.IsWellFormedUriString(endereco, UriKind.Absolute) ? endereco : string.Empty;
        }

        private static DateTime? DataDePublicacao(JToken valor)
        {
            if (valor == null)
                return null;

            if (valor.Type == JTokenType.Date)
                return valor.ToObject<DateTime>().ToUniversalTime();

            return DateTime.TryParse(valor.ToString(), CultureInfo.InvariantCulture,
                                     DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out DateTime data)
                ? data
                : (DateTime?)null;
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
