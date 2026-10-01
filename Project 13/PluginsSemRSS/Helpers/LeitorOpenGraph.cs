using System;
using System.Net;
using System.Text.RegularExpressions;

namespace TVPlayerAPI.Helpers
{
    internal static class LeitorOpenGraph
    {
        private static readonly Regex REGEX_META =
            new Regex("<meta\\s[^>]*>", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex REGEX_ATRIBUTO =
            new Regex("(?<nome>property|name|content)\\s*=\\s*\"(?<valor>[^\"]*)\"", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        internal static string Valor(string html, string propriedade)
        {
            if (string.IsNullOrEmpty(html))
                return null;

            foreach (Match meta in REGEX_META.Matches(html))
            {
                string nome = null;
                string conteudo = null;

                foreach (Match atributo in REGEX_ATRIBUTO.Matches(meta.Value))
                {
                    if (atributo.Groups["nome"].Value.Equals("content", StringComparison.OrdinalIgnoreCase))
                        conteudo = atributo.Groups["valor"].Value;
                    else
                        nome = atributo.Groups["valor"].Value;
                }

                if (conteudo != null && propriedade.Equals(nome, StringComparison.OrdinalIgnoreCase))
                {
                    string valor = WebUtility.HtmlDecode(conteudo).Trim();
                    return valor.Length == 0 ? null : valor;
                }
            }

            return null;
        }
    }
}
