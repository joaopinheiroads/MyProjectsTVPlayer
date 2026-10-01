using System.Text.RegularExpressions;

namespace TVPlayerAPI.G1.Parsers
{
    internal static class FalkorInstanciaParser
    {
        private static readonly Regex REGEX_INSTANCIA =
            new Regex(@"falkor-cda\.bastian\.globo\.com/tenants/g1/instances/(?<id>[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12})",
                      RegexOptions.IgnoreCase | RegexOptions.Compiled);

        internal static string InstanciaDaPagina(string html)
        {
            if (string.IsNullOrEmpty(html))
                return null;

            Match instancia = REGEX_INSTANCIA.Match(html);

            return instancia.Success ? instancia.Groups["id"].Value.ToLowerInvariant() : null;
        }
    }
}
