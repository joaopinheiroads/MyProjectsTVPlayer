using System.Net;
using System.Text.RegularExpressions;

namespace TVPlayerAPI.ICarros.Parsers
{
    internal static class MateriaICarrosParser
    {
        private static readonly Regex REGEX_AUTOR =
            new Regex(@"<p class=""dados"">\s*\d{2}/\d{2}/\d{4}\s*-\s*(?<autor>[^/<]*?)\s*/\s*Fonte",
                      RegexOptions.IgnoreCase | RegexOptions.Compiled);

        internal static string Autor(string html)
        {
            if (string.IsNullOrEmpty(html))
                return null;

            Match autor = REGEX_AUTOR.Match(html);

            return autor.Success ? WebUtility.HtmlDecode(autor.Groups["autor"].Value).Trim() : null;
        }
    }
}
