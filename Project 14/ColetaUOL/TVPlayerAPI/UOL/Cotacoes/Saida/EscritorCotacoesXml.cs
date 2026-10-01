using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using TVPlayerAPI.UOL.Cotacoes.Classes;

namespace TVPlayerAPI.UOL.Cotacoes.Saida
{
    internal static class EscritorCotacoesXml
    {
        private static readonly XNamespace XSI = "http://www.w3.org/2001/XMLSchema-instance";
        private static readonly XNamespace XSD = "http://www.w3.org/2001/XMLSchema";

        internal static string Escrever(IEnumerable<LinhaCotacao> linhas)
        {
            var canal = new XElement("channel",
                linhas.Select(linha =>
                    new XElement("item",
                        new XElement("nome", linha.Nome),
                        new XElement("compra", linha.Compra),
                        linha.Venda == null ? null : new XElement("venda", linha.Venda),
                        new XElement("variacao", linha.Variacao),
                        new XElement("pubDate", linha.PublicacaoUtc.ToString("r", CultureInfo.InvariantCulture)))));

            return new XDocument(
                new XElement("rss",
                    new XAttribute(XNamespace.Xmlns + "xsi", XSI),
                    new XAttribute(XNamespace.Xmlns + "xsd", XSD),
                    new XAttribute("version", "2.0"),
                    canal)).ToString();
        }
    }
}
