using NLog;
using SerializableObjects.PluginsTVPlayer;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TVPlayerAPI.Helpers;
using TVPlayerAPI.ICarros.Classes;
using TVPlayerAPI.ICarros.Clients;
using TVPlayerAPI.ICarros.Interfaces;
using TVPlayerAPI.ICarros.Saida;

namespace TVPlayerAPI.ICarros.Services
{
    internal class ICarrosService
    {
        private const int MAX_ITENS = 15;

        private const int PAUSA_ENTRE_MATERIAS_MS = 200;

        private static readonly HttpDedicado http = new HttpDedicado("ICarros");

        private readonly IFonteNoticiaICarros fonte;

        private readonly IFonteAutorICarros fonteAutor;

        private readonly Logger logger = LogManager.GetCurrentClassLogger();

        internal ICarrosService(IFonteNoticiaICarros fonte, IFonteAutorICarros fonteAutor)
        {
            this.fonte = fonte;
            this.fonteAutor = fonteAutor;
        }

        internal static Task<KeyValuePair<string, RssRecoverType>> RecuperarAsync()
        {
            return new ICarrosService(new ICarrosArquivoClient(http), new ICarrosMateriaClient(http)).ColetarAsync();
        }

        internal async Task<KeyValuePair<string, RssRecoverType>> ColetarAsync()
        {
            try
            {
                IReadOnlyList<NoticiaICarros> maisNovas = await fonte.ObterNoticiasAsync(MAX_ITENS).ConfigureAwait(false);

                if (maisNovas.Count == 0)
                {
                    logger.Warn("iCarros: a fonte \"" + fonte.Nome + "\" nao trouxe noticia; a pasta mantem o conteudo anterior.");
                    return new KeyValuePair<string, RssRecoverType>(EscritorFeedICarrosXml.Escrever(new List<NoticiaICarros>()), RssRecoverType.SemConteudo);
                }

                IReadOnlyList<NoticiaICarros> comAutor = await ComAutorDaMateriaAsync(maisNovas).ConfigureAwait(false);

                logger.Info("iCarros: coletou " + comAutor.Count + " de " + MAX_ITENS + " noticias pela fonte \"" + fonte.Nome + "\".");

                return new KeyValuePair<string, RssRecoverType>(EscritorFeedICarrosXml.Escrever(comAutor), RssRecoverType.Sucesso);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "iCarros: falha ao coletar pela fonte \"" + fonte.Nome + "\".");
                return new KeyValuePair<string, RssRecoverType>(string.Empty, RssRecoverType.Erro);
            }
        }

        private async Task<IReadOnlyList<NoticiaICarros>> ComAutorDaMateriaAsync(IReadOnlyList<NoticiaICarros> noticias)
        {
            var comAutor = new List<NoticiaICarros>();

            foreach (NoticiaICarros noticia in noticias)
            {
                string autor = await fonteAutor.ObterAutorAsync(noticia.Link).ConfigureAwait(false);

                if (autor == null)
                    logger.Warn("iCarros: autor nao encontrado, segue sem autor: " + noticia.Link);

                comAutor.Add(noticia.ComAutor(autor ?? string.Empty));

                await Task.Delay(PAUSA_ENTRE_MATERIAS_MS).ConfigureAwait(false);
            }

            return comAutor;
        }
    }
}
