using NLog;
using SerializableObjects.PluginsTVPlayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TVPlayerAPI.Helpers;
using TVPlayerAPI.UOL.Cotacoes.Classes;
using TVPlayerAPI.UOL.Cotacoes.Clients;
using TVPlayerAPI.UOL.Cotacoes.Interfaces;
using TVPlayerAPI.UOL.Cotacoes.Saida;

namespace TVPlayerAPI.UOL.Cotacoes.Services
{
    internal class UOLCotacoesService
    {
        private static readonly HttpDedicado http = new HttpDedicado("UOL.Cotacoes");

        private readonly IFonteCotacoesUOL fonte;

        private readonly Logger logger = LogManager.GetCurrentClassLogger();

        internal UOLCotacoesService(IFonteCotacoesUOL fonte)
        {
            this.fonte = fonte;
        }

        internal static Task<KeyValuePair<string, RssRecoverType>> RecuperarAsync()
        {
            return new UOLCotacoesService(new CotacoesUOLApiClient(http)).ColetarAsync();
        }

        internal async Task<KeyValuePair<string, RssRecoverType>> ColetarAsync()
        {
            try
            {
                var daFonte = new Dictionary<IndicadorCotacao, CotacaoUOL>();

                foreach (IndicadorCotacao indicador in IndicadorCotacao.NaOrdemDaTela.Where(indicador => indicador.VemDaFonte))
                {
                    CotacaoUOL cotacao = await ObterAsync(indicador).ConfigureAwait(false);

                    if (cotacao == null)
                    {
                        logger.Warn("UOLCotacoes: a " + fonte.Nome + " nao trouxe " + indicador.NomeNaTela + "; a pasta mantem o conteudo anterior.");
                        return Falhou();
                    }

                    daFonte.Add(indicador, cotacao);
                }

                DateTime maisRecente = daFonte.Values.Max(cotacao => cotacao.MomentoUtc);
                List<LinhaCotacao> linhas = IndicadorCotacao.NaOrdemDaTela.Select(indicador => Linha(indicador, daFonte, maisRecente)).ToList();

                logger.Info("UOLCotacoes: coletou " + daFonte.Count + " cotacoes da " + fonte.Nome + "; a mais recente e de "
                            + maisRecente.ToLocalTime().ToString("dd/MM/yyyy HH:mm") + ".");

                return new KeyValuePair<string, RssRecoverType>(EscritorCotacoesXml.Escrever(linhas), RssRecoverType.Sucesso);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "UOLCotacoes: falha ao coletar as cotacoes do site.");
                return Falhou();
            }
        }

        private Task<CotacaoUOL> ObterAsync(IndicadorCotacao indicador)
        {
            return indicador.Tipo == TipoIndicador.Moeda
                ? fonte.ObterMoedaAsync(indicador.IdNaFonte)
                : fonte.ObterIndiceAsync(indicador.IdNaFonte);
        }

        private static LinhaCotacao Linha(IndicadorCotacao indicador, IDictionary<IndicadorCotacao, CotacaoUOL> daFonte, DateTime maisRecente)
        {
            switch (indicador.Tipo)
            {
                case TipoIndicador.Moeda:
                    return LinhaCotacao.DeMoeda(indicador.NomeNaTela, daFonte[indicador]);
                case TipoIndicador.Indice:
                    return LinhaCotacao.DeIndice(indicador.NomeNaTela, daFonte[indicador]);
                case TipoIndicador.DolarParalelo:
                    return LinhaCotacao.DolarParalelo(indicador.NomeNaTela);
                default:
                    return LinhaCotacao.Descontinuada(indicador.NomeNaTela, maisRecente);
            }
        }

        private static KeyValuePair<string, RssRecoverType> Falhou()
        {
            return new KeyValuePair<string, RssRecoverType>(string.Empty, RssRecoverType.Erro);
        }
    }
}
