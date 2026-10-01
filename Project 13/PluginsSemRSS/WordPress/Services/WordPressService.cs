using NLog;
using SerializableObjects.PluginsTVPlayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TVPlayerAPI.Helpers;
using TVPlayerAPI.WordPress.Classes;
using TVPlayerAPI.WordPress.Clients;
using TVPlayerAPI.WordPress.Interfaces;
using TVPlayerAPI.WordPress.Saida;

namespace TVPlayerAPI.WordPress.Services
{
    internal class WordPressService
    {
        private const int MAX_ITENS = 15;

        private const int FOLGA_PARA_DESCARTES = 10;

        private static readonly HttpDedicado http = new HttpDedicado("WordPress");

        private readonly IFonteNoticiaWordPress fonte;

        private readonly Logger logger = LogManager.GetCurrentClassLogger();

        internal WordPressService(IFonteNoticiaWordPress fonte)
        {
            this.fonte = fonte;
        }

        internal static bool Atende(string urlDoPlugin)
        {
            return SiteWordPress.DoPlugin(urlDoPlugin) != null;
        }

        internal static Task<KeyValuePair<string, RssRecoverType>> RecuperarAsync(string urlDoPlugin)
        {
            return new WordPressService(new WordPressPostsClient(http)).ColetarAsync(urlDoPlugin);
        }

        internal async Task<KeyValuePair<string, RssRecoverType>> ColetarAsync(string urlDoPlugin)
        {
            SiteWordPress site = SiteWordPress.DoPlugin(urlDoPlugin);

            if (site == null)
                return new KeyValuePair<string, RssRecoverType>(string.Empty, RssRecoverType.NaoExiste);

            try
            {
                IReadOnlyList<NoticiaWordPress> recebidas = await fonte.ObterNoticiasAsync(site, MAX_ITENS + FOLGA_PARA_DESCARTES).ConfigureAwait(false);
                IReadOnlyList<NoticiaWordPress> completas = MaisNovasCompletas(recebidas);

                if (completas.Count == 0)
                {
                    logger.Warn("WordPress: " + site.Canal + " (" + urlDoPlugin + ") nao trouxe noticia aproveitavel pela fonte \"" + fonte.Nome + "\"; a pasta mantem o conteudo anterior.");
                    return new KeyValuePair<string, RssRecoverType>(EscritorFeedWordPressXml.Escrever(site.Canal, new List<NoticiaWordPress>()), RssRecoverType.SemConteudo);
                }

                if (completas.Count < MAX_ITENS)
                    logger.Warn("WordPress: " + site.Canal + " aproveitou so " + completas.Count + " de " + recebidas.Count + " noticias; o merge completa com as da pasta.");
                else
                    logger.Info("WordPress: " + site.Canal + " (" + urlDoPlugin + ") coletou " + completas.Count + " noticias pela fonte \"" + fonte.Nome + "\".");

                return new KeyValuePair<string, RssRecoverType>(EscritorFeedWordPressXml.Escrever(site.Canal, completas), RssRecoverType.Sucesso);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "WordPress: falha ao coletar " + site.Canal + " (" + urlDoPlugin + ") pela fonte \"" + fonte.Nome + "\".");
                return new KeyValuePair<string, RssRecoverType>(string.Empty, RssRecoverType.Erro);
            }
        }

        private IReadOnlyList<NoticiaWordPress> MaisNovasCompletas(IReadOnlyList<NoticiaWordPress> recebidas)
        {
            foreach (NoticiaWordPress incompleta in recebidas.Where(noticia => !noticia.Completa))
                logger.Warn("WordPress: noticia sem imagem ou sem resumo ficou de fora: " + incompleta.Link);

            return recebidas.Where(noticia => noticia.Completa)
                            .GroupBy(noticia => noticia.Link, StringComparer.OrdinalIgnoreCase)
                            .Select(mesmoLink => mesmoLink.First())
                            .OrderByDescending(noticia => noticia.Publicacao)
                            .Take(MAX_ITENS)
                            .ToList();
        }
    }
}
