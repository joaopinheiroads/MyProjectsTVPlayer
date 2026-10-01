using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net;
using TVPlayerAPI.GazetaDoPovo.Classes;
using TVPlayerAPI.Helpers;

namespace TVPlayerAPI.GazetaDoPovo.Parsers
{
    internal static class ListagemGazetaParser
    {
        internal static ListagemGazeta Ler(string html)
        {
            JObject lista = LeitorLdJson.PrimeiroDoTipo(html, "ItemList");

            return new ListagemGazeta(TextoLimpo(lista?["name"]), LinksDaLista(lista));
        }

        private static List<Uri> LinksDaLista(JObject lista)
        {
            var links = new List<Uri>();

            if (!(lista?["itemListElement"] is JArray elementos))
                return links;

            foreach (JToken elemento in elementos)
            {
                string endereco = TextoLimpo((elemento as JObject)?["url"]);

                if (endereco != null
                    && Uri.TryCreate(SecaoGazeta.RAIZ_DO_SITE, endereco, out Uri absoluto)
                    && absoluto.Host.Equals(SecaoGazeta.HOST, StringComparison.OrdinalIgnoreCase)
                    && !links.Contains(absoluto))
                    links.Add(absoluto);
            }

            return links;
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
