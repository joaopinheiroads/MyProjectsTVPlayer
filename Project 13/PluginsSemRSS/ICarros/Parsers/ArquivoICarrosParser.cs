using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Text.RegularExpressions;
using TVPlayerAPI.ICarros.Classes;

namespace TVPlayerAPI.ICarros.Parsers
{
    internal static class ArquivoICarrosParser
    {
        private const string INICIO_DO_BLOCO = "<ul class=\"listahorizontalarquivo";

        private const string FORMATO_DATA = "dd/MM/yyyy";

        private static readonly Uri RAIZ_DO_SITE = new Uri("https://www.icarros.com.br/");

        private static readonly Regex REGEX_LINK =
            new Regex("href=\"(?<link>/noticias/[^\"]+/\\d+\\.html)\"", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex REGEX_FOTO =
            new Regex(@"imgnoticia/\d+/(?<codigo>\d+_\d+)(?:\.[a-z]+)?""", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex REGEX_MANCHETE =
            new Regex(@"<h1>(?<texto>[^<]+)</h1>", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex REGEX_DATA =
            new Regex(@"postado em:\s*(?<data>\d{2}/\d{2}/\d{4})", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        internal static IReadOnlyList<NoticiaICarros> Noticias(string html)
        {
            var noticias = new List<NoticiaICarros>();

            if (string.IsNullOrEmpty(html))
                return noticias;

            string[] blocos = html.Split(new[] { INICIO_DO_BLOCO }, StringSplitOptions.None);

            for (int i = 1; i < blocos.Length; i++)
            {
                NoticiaICarros noticia = Noticia(blocos[i]);

                if (noticia != null)
                    noticias.Add(noticia);
            }

            return noticias;
        }

        private static NoticiaICarros Noticia(string bloco)
        {
            Match link = REGEX_LINK.Match(bloco);
            Match foto = REGEX_FOTO.Match(bloco);
            Match manchete = REGEX_MANCHETE.Match(bloco);
            Match data = REGEX_DATA.Match(bloco);

            if (!link.Success || !foto.Success || !manchete.Success || !data.Success)
                return null;

            string texto = WebUtility.HtmlDecode(manchete.Groups["texto"].Value).Trim();

            if (texto.Length == 0
                || !DateTime.TryParseExact(data.Groups["data"].Value, FORMATO_DATA, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime publicacao))
                return null;

            return new NoticiaICarros(texto,
                                      new Uri(RAIZ_DO_SITE, link.Groups["link"].Value).AbsoluteUri,
                                      foto.Groups["codigo"].Value,
                                      publicacao,
                                      null);
        }
    }
}
