using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using TVPlayerAPI.TVFoco.Classes;

namespace TVPlayerAPI.TVFoco.Parsers
{
    internal static class SitemapNewsTVFocoParser
    {
        internal const string HOST = "tvfoco.uai.com.br";

        internal static IReadOnlyList<EntradaSitemapTVFoco> Materias(string xml)
        {
            if (string.IsNullOrEmpty(xml))
                return new List<EntradaSitemapTVFoco>();

            return XDocument.Parse(xml)
                .Descendants()
                .Where(elemento => elemento.Name.LocalName == "url")
                .Select(LerEntrada)
                .Where(entrada => entrada != null)
                .ToList();
        }

        private static EntradaSitemapTVFoco LerEntrada(XElement url)
        {
            string endereco = ValorDoDescendente(url, "loc");
            string publicacao = ValorDoDescendente(url, "publication_date");

            if (!Uri.TryCreate(endereco, UriKind.Absolute, out Uri uri)
                || !uri.Host.Equals(HOST, StringComparison.OrdinalIgnoreCase))
                return null;

            DateTime.TryParse(publicacao, CultureInfo.InvariantCulture,
                              DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out DateTime quando);

            return new EntradaSitemapTVFoco(uri, quando);
        }

        private static string ValorDoDescendente(XElement pai, string nomeLocal)
        {
            return pai.Descendants().FirstOrDefault(filho => filho.Name.LocalName == nomeLocal)?.Value.Trim();
        }
    }
}
