using Newtonsoft.Json;
using NLog;
using System;
using System.Threading.Tasks;
using TVPlayerAPI.Helpers;
using TVPlayerAPI.UOL.Cotacoes.Classes;
using TVPlayerAPI.UOL.Cotacoes.Interfaces;
using TVPlayerAPI.UOL.Cotacoes.Parsers;

namespace TVPlayerAPI.UOL.Cotacoes.Clients
{
    internal class CotacoesUOLApiClient : IFonteCotacoesUOL
    {
        private const string HOST = "https://api.cotacoes.uol.com";
        private const string URL_MOEDA = HOST + "/currency/summary?currency={0}&fields=" + ResumoCotacaoParser.CAMPOS_MOEDA;
        private const string URL_INDICE = HOST + "/asset/summary?item={0}&fields=" + ResumoCotacaoParser.CAMPOS_INDICE;

        private readonly HttpDedicado http;

        private readonly Logger logger = LogManager.GetCurrentClassLogger();

        internal CotacoesUOLApiClient(HttpDedicado http)
        {
            this.http = http;
        }

        public string Nome
        {
            get { return "api de cotacoes do site"; }
        }

        public async Task<CotacaoUOL> ObterMoedaAsync(int idMoeda)
        {
            string url = string.Format(URL_MOEDA, idMoeda);

            return Interpretar(url, ResumoCotacaoParser.Moeda, await http.ObterTextoAsync(url).ConfigureAwait(false));
        }

        public async Task<CotacaoUOL> ObterIndiceAsync(int idIndice)
        {
            string url = string.Format(URL_INDICE, idIndice);

            return Interpretar(url, ResumoCotacaoParser.Indice, await http.ObterTextoAsync(url).ConfigureAwait(false));
        }

        private CotacaoUOL Interpretar(string url, Func<string, CotacaoUOL> parser, string json)
        {
            try
            {
                CotacaoUOL cotacao = parser(json);

                if (cotacao == null && !string.IsNullOrEmpty(json))
                    logger.Warn("UOLCotacoes: a resposta veio sem valor, variacao ou data: " + url);

                return cotacao;
            }
            catch (JsonException ex)
            {
                logger.Warn(ex, "UOLCotacoes: a resposta nao e um JSON que da para ler: " + url);
                return null;
            }
        }
    }
}
