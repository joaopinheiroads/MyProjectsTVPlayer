using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using TVPlayerAPI.Tecmundo.Classes;

namespace TVPlayerAPI.Tecmundo.Parsers
{
    internal static class SitemapEstadaoParser
    {
        private const string HOST_DO_ESTADAO = "www.estadao.com.br";
        private const string CAMINHO_DA_SECAO = "/tecmundo/";
        private const string CAMINHO_FORA_DE_NOTICIA = "/tecmundo/guia-de-compras/";

        internal static IReadOnlyList<EntradaSitemapTecmundo> NoticiasDoTecmundo(string xml)
        {
            if (string.IsNullOrEmpty(xml))
                return new List<EntradaSitemapTecmundo>();

            return XDocument.Parse(xml)
                .Descendants()
                .Where(elemento => elemento.Name.LocalName == "url")
                .Select(LerEntrada)
                .Where(entrada => entrada != null && EhNoticiaDoTecmundo(entrada.Endereco))
                .ToList();
        }

        private static EntradaSitemapTecmundo LerEntrada(XElement url)
        {
            string endereco = ValorDoFilho(url, "loc");
            string atualizacao = ValorDoFilho(url, "lastmod");

            if (!Uri.TryCreate(endereco, UriKind.Absolute, out Uri uri))
                return null;

            DateTime.TryParse(atualizacao, CultureInfo.InvariantCulture,
                              DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out DateTime quando);

            return new EntradaSitemapTecmundo(uri, quando);
        }

        private static string ValorDoFilho(XElement pai, string nomeLocal)
        {
            return pai.Elements().FirstOrDefault(filho => filho.Name.LocalName == nomeLocal)?.Value.Trim();
        }

        private static bool EhNoticiaDoTecmundo(Uri endereco)
        {
            string caminho = endereco.AbsolutePath;

            return endereco.Host.Equals(HOST_DO_ESTADAO, StringComparison.OrdinalIgnoreCase)
                && caminho.StartsWith(CAMINHO_DA_SECAO, StringComparison.OrdinalIgnoreCase)
                && caminho.Length > CAMINHO_DA_SECAO.Length
                && !caminho.StartsWith(CAMINHO_FORA_DE_NOTICIA, StringComparison.OrdinalIgnoreCase);
        }
    }
}
