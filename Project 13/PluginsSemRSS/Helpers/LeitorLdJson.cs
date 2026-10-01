using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace TVPlayerAPI.Helpers
{
    internal static class LeitorLdJson
    {
        private static readonly Regex REGEX_BLOCO =
            new Regex("<script[^>]*type=\"application/ld\\+json\"[^>]*>(.*?)</script>",
                      RegexOptions.Singleline | RegexOptions.IgnoreCase | RegexOptions.Compiled);

        internal static IEnumerable<JObject> Objetos(string html)
        {
            if (string.IsNullOrEmpty(html))
                yield break;

            foreach (Match bloco in REGEX_BLOCO.Matches(html))
            {
                JToken raiz = Interpretar(bloco.Groups[1].Value);

                foreach (JObject objeto in Achatar(raiz))
                    yield return objeto;
            }
        }

        internal static JObject PrimeiroDoTipo(string html, string tipo)
        {
            return Objetos(html).FirstOrDefault(objeto => Tipos(objeto).Contains(tipo));
        }

        internal static JObject PrimeiroArtigo(string html)
        {
            return Objetos(html).FirstOrDefault(objeto => Tipos(objeto).Any(tipo => tipo.EndsWith("Article")));
        }

        internal static IEnumerable<string> Tipos(JObject objeto)
        {
            JToken tipo = objeto["@type"];

            if (tipo is JArray tipos)
                return tipos.Select(t => t.ToString());

            return tipo == null ? Enumerable.Empty<string>() : new[] { tipo.ToString() };
        }

        private static JToken Interpretar(string json)
        {
            try
            {
                return JToken.Parse(json);
            }
            catch (JsonException)
            {
                return null;
            }
        }

        private static IEnumerable<JObject> Achatar(JToken token)
        {
            if (token is JArray lista)
            {
                foreach (JToken filho in lista)
                {
                    foreach (JObject neto in Achatar(filho))
                        yield return neto;
                }

                yield break;
            }

            if (!(token is JObject objeto))
                yield break;

            yield return objeto;

            foreach (JObject noGrafo in Achatar(objeto["@graph"]))
                yield return noGrafo;
        }
    }
}
