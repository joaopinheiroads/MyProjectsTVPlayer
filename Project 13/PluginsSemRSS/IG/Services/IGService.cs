using NLog;
using SerializableObjects.PluginsTVPlayer;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TVPlayerAPI.Helpers;
using TVPlayerAPI.IG.Classes;
using TVPlayerAPI.IG.Clients;
using TVPlayerAPI.IG.Interfaces;
using TVPlayerAPI.IG.Saida;

namespace TVPlayerAPI.IG.Services
{
    internal class IGService
    {
        private const int MAX_ITENS = 15;

        private static readonly HttpDedicado http = new HttpDedicado("IG");

        private readonly IFonteNoticiaIG fonte;

        private readonly Logger logger = LogManager.GetCurrentClassLogger();

        internal IGService(IFonteNoticiaIG fonte)
        {
            this.fonte = fonte;
        }

        internal static Task<KeyValuePair<string, RssRecoverType>> RecuperarAsync(RSSType tipo)
        {
            return new IGService(new IGContentApiClient(http)).ColetarAsync(tipo);
        }

        internal async Task<KeyValuePair<string, RssRecoverType>> ColetarAsync(RSSType tipo)
        {
            IGCanal canal = IGCanal.Por(tipo);

            if (canal == null)
                return new KeyValuePair<string, RssRecoverType>(string.Empty, RssRecoverType.NaoExiste);

            try
            {
                IReadOnlyList<NoticiaIG> noticias = await fonte.ObterNoticiasAsync(canal, MAX_ITENS).ConfigureAwait(false);

                if (noticias.Count == 0)
                {
                    logger.Warn("IG: " + canal.Nome + " nao trouxe noticia aproveitavel pela fonte \"" + fonte.Nome + "\"; a pasta mantem o conteudo anterior.");
                    return SemConteudo(canal);
                }

                if (noticias.Count < MAX_ITENS)
                    logger.Info("IG: " + canal.Nome + " aproveitou " + noticias.Count + " de " + MAX_ITENS + " noticias; o merge completa o resto.");
                else
                    logger.Info("IG: " + canal.Nome + " atualizado com " + noticias.Count + " noticias pela fonte \"" + fonte.Nome + "\".");

                return new KeyValuePair<string, RssRecoverType>(EscritorFeedXml.Escrever(canal.Nome, noticias), RssRecoverType.Sucesso);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "IG: falha ao coletar " + canal.Nome + " pela fonte \"" + fonte.Nome + "\".");
                return new KeyValuePair<string, RssRecoverType>(string.Empty, RssRecoverType.Erro);
            }
        }

        private static KeyValuePair<string, RssRecoverType> SemConteudo(IGCanal canal)
        {
            return new KeyValuePair<string, RssRecoverType>(EscritorFeedXml.Escrever(canal.Nome, new List<NoticiaIG>()), RssRecoverType.SemConteudo);
        }
    }
}
