using NLog;
using SerializableObjects.PluginsTVPlayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TVPlayerAPI.GazetaDoPovo.Classes;
using TVPlayerAPI.GazetaDoPovo.Clients;
using TVPlayerAPI.GazetaDoPovo.Interfaces;
using TVPlayerAPI.GazetaDoPovo.Saida;
using TVPlayerAPI.Helpers;

namespace TVPlayerAPI.GazetaDoPovo.Services
{
    internal class GazetaDoPovoService
    {
        private const int MAX_ITENS = 15;
        private const int PAUSA_ENTRE_MATERIAS_MS = 200;

        private static readonly HttpDedicado http = new HttpDedicado("GazetaDoPovo");

        private readonly IFonteNoticiaGazeta fonte;

        private readonly Logger logger = LogManager.GetCurrentClassLogger();

        internal GazetaDoPovoService(IFonteNoticiaGazeta fonte)
        {
            this.fonte = fonte;
        }

        internal static bool Atende(string urlDoPlugin)
        {
            return SecaoGazeta.DoPlugin(urlDoPlugin) != null;
        }

        internal static Task<KeyValuePair<string, RssRecoverType>> RecuperarAsync(string urlDoPlugin)
        {
            return new GazetaDoPovoService(new GazetaDoPovoSiteClient(http)).ColetarAsync(urlDoPlugin);
        }

        internal async Task<KeyValuePair<string, RssRecoverType>> ColetarAsync(string urlDoPlugin)
        {
            SecaoGazeta secao = SecaoGazeta.DoPlugin(urlDoPlugin);

            if (secao == null)
                return new KeyValuePair<string, RssRecoverType>(string.Empty, RssRecoverType.NaoExiste);

            try
            {
                ListagemGazeta listagem = await fonte.ObterListagemAsync(secao).ConfigureAwait(false);
                string nomeDaSecao = listagem.Nome ?? secao.Slug;

                if (listagem.Links.Count == 0)
                {
                    logger.Warn("Gazeta do Povo: a listagem " + secao.UrlDaListagem + " nao trouxe materias; a pasta mantem o conteudo anterior.");
                    return SemConteudo(nomeDaSecao);
                }

                List<MateriaGazeta> materias = await ObterMateriasAsync(listagem.Links.Take(MAX_ITENS)).ConfigureAwait(false);

                if (materias.Count == 0)
                {
                    logger.Warn("Gazeta do Povo: nenhuma materia de " + nomeDaSecao + " trouxe manchete e imagem; a pasta mantem o conteudo anterior.");
                    return SemConteudo(nomeDaSecao);
                }

                logger.Info("Gazeta do Povo: " + nomeDaSecao + " coletou " + materias.Count + " de " + Math.Min(listagem.Links.Count, MAX_ITENS) + " materias da listagem.");

                List<MateriaGazeta> maisNovasPrimeiro = materias.OrderByDescending(materia => materia.Publicacao).ToList();

                return new KeyValuePair<string, RssRecoverType>(EscritorFeedGazetaXml.Escrever(nomeDaSecao, maisNovasPrimeiro), RssRecoverType.Sucesso);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Gazeta do Povo: falha ao coletar " + secao.UrlDaListagem + ".");
                return new KeyValuePair<string, RssRecoverType>(string.Empty, RssRecoverType.Erro);
            }
        }

        private async Task<List<MateriaGazeta>> ObterMateriasAsync(IEnumerable<Uri> links)
        {
            var materias = new List<MateriaGazeta>();

            foreach (Uri link in links)
            {
                MateriaGazeta materia = await fonte.ObterMateriaAsync(link).ConfigureAwait(false);

                if (materia != null)
                    materias.Add(materia);

                await Task.Delay(PAUSA_ENTRE_MATERIAS_MS).ConfigureAwait(false);
            }

            return materias;
        }

        private static KeyValuePair<string, RssRecoverType> SemConteudo(string nomeDaSecao)
        {
            return new KeyValuePair<string, RssRecoverType>(EscritorFeedGazetaXml.Escrever(nomeDaSecao, new List<MateriaGazeta>()), RssRecoverType.SemConteudo);
        }
    }
}
