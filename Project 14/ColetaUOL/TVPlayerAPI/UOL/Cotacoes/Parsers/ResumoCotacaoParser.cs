using Newtonsoft.Json.Linq;
using System;
using System.Globalization;
using System.Linq;
using TVPlayerAPI.UOL.Cotacoes.Classes;

namespace TVPlayerAPI.UOL.Cotacoes.Parsers
{
    internal static class ResumoCotacaoParser
    {
        internal const string CAMPOS_MOEDA = "name,bidvalue,askvalue,variationpercentbid,date";
        internal const string CAMPOS_INDICE = "abbreviation,price,pctChange,date";

        private const string FORMATO_DATA_DA_FONTE = "yyyyMMddHHmmss";

        private static readonly TimeSpan FUSO_DE_BRASILIA = TimeSpan.FromHours(-3);

        internal static CotacaoUOL Moeda(string json)
        {
            return Ler(json, "bidvalue", "askvalue", "variationpercentbid");
        }

        internal static CotacaoUOL Indice(string json)
        {
            return Ler(json, null, "price", "pctChange");
        }

        private static CotacaoUOL Ler(string json, string campoCompra, string campoValor, string campoVariacao)
        {
            if (string.IsNullOrEmpty(json))
                return null;

            JObject documento = (JObject.Parse(json)["docs"] as JArray)?.OfType<JObject>().FirstOrDefault();

            decimal? valor = Numero(documento?[campoValor]);
            decimal? variacao = Numero(documento?[campoVariacao]);
            DateTime? momento = MomentoUtc(documento?["date"]);

            if (valor == null || variacao == null || momento == null)
                return null;

            decimal? compra = campoCompra == null ? null : Numero(documento[campoCompra]);

            return new CotacaoUOL(compra, valor.Value, variacao.Value, momento.Value);
        }

        private static decimal? Numero(JToken valor)
        {
            if (valor == null || (valor.Type != JTokenType.Float && valor.Type != JTokenType.Integer))
                return null;

            return valor.Value<decimal>();
        }

        private static DateTime? MomentoUtc(JToken valor)
        {
            if (valor == null)
                return null;

            if (!DateTime.TryParseExact(valor.ToString(), FORMATO_DATA_DA_FONTE, CultureInfo.InvariantCulture,
                                        DateTimeStyles.None, out DateTime horaDeBrasilia))
                return null;

            return new DateTimeOffset(horaDeBrasilia, FUSO_DE_BRASILIA).UtcDateTime;
        }
    }
}
