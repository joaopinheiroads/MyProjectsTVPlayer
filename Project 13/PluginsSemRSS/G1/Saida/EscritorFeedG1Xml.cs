using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using TVPlayerAPI.G1.Classes;

namespace TVPlayerAPI.G1.Saida
{
    internal static class EscritorFeedG1Xml
    {
        private const string PREFIXO_AUTOR_DO_RSSCUSTOM = "id-";

        internal static string Escrever(string canal, IEnumerable<NoticiaG1> noticias)
        {
            var elementoCanal = new XElement("channel",
                new XElement("title", canal),
                noticias.Select(noticia =>
                    new XElement("item",
                        new XElement("title", noticia.Manchete),
                        new XElement("description", noticia.Resumo),
                        new XElement("enclosure",
                            new XAttribute("type", "image"),
                            new XAttribute("url", noticia.Imagem)),
                        new XElement("author", PREFIXO_AUTOR_DO_RSSCUSTOM + DateTime.UtcNow.Ticks))));

            return new XDocument(
                new XElement("rss",
                    new XAttribute("version", "2.0"),
                    elementoCanal)).ToString();
        }
    }
}
