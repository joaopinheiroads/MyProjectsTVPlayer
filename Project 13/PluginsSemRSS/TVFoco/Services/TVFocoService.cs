using NLog;
using SerializableObjects.PluginsTVPlayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TVPlayerAPI.Helpers;
using TVPlayerAPI.TVFoco.Classes;
using TVPlayerAPI.TVFoco.Clients;
using TVPlayerAPI.TVFoco.Interfaces;
using TVPlayerAPI.TVFoco.Saida;

namespace TVPlayerAPI.TVFoco.Services
{
    internal class TVFocoService
    {
        private const int MAX_ITENS = 15;
        private const int PAUSA_ENTRE_MATERIAS_MS = 200;

        private static readonly HttpDedicado http = new HttpDedicado("TVFoco");

        private readonly IFonteNoticiaTVFoco fonte;

        private readonly Logger logger = LogManager.GetCurrentClassLogger();

        internal TVFocoService(IFonteNoticiaTVFoco fonte)
        {
            this.fonte = fonte;
        }

        internal static Task<KeyValuePair<string, RssRecoverType>> RecuperarAsync()
        {
            return new TVFocoService(new TVFocoSiteClient(http)).ColetarAsync();
        }

        internal async Task<KeyValuePair<string, RssRecoverType>> ColetarAsync()
        {
            try
            {
                List<EntradaSitemapTVFoco> links = MaisRecentesSemRepeticao(await fonte.ObterEntradasAsync().ConfigureAwait(false));

                if (links.Count == 0)
                {
                    logger.Warn("TVFoco: o " + fonte.Nome + " nao trouxe materias; a pasta mantem o conteudo anterior.");
                    return SemConteudo();
                }

                List<MateriaTVFoco> materias = await ObterMateriasAsync(links).ConfigureAwait(false);

                if (materias.Count == 0)
                {
                    logger.Warn("TVFoco: nenhuma das " + links.Count + " materias trouxe titulo e data; a pasta mantem o conteudo anterior.");
                    return SemConteudo();
                }

                logger.Info("TVFoco: coletou " + materias.Count + " de " + links.Count + " materias do " + fonte.Nome + ".");

                List<MateriaTVFoco> maisNovasPrimeiro = materias.OrderByDescending(materia => materia.Publicacao).ToList();

                return new KeyValuePair<string, RssRecoverType>(EscritorFeedTVFocoXml.Escrever(maisNovasPrimeiro), RssRecoverType.Sucesso);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "TVFoco: falha ao coletar as noticias do site.");
                return new KeyValuePair<string, RssRecoverType>(string.Empty, RssRecoverType.Erro);
            }
        }

        private static List<EntradaSitemapTVFoco> MaisRecentesSemRepeticao(IEnumerable<EntradaSitemapTVFoco> entradas)
        {
            return entradas
                .GroupBy(entrada => entrada.Endereco)
                .Select(grupo => new EntradaSitemapTVFoco(grupo.Key, grupo.Max(entrada => entrada.Publicacao)))
                .OrderByDescending(entrada => entrada.Publicacao)
                .Take(MAX_ITENS)
                .ToList();
        }

        private async Task<List<MateriaTVFoco>> ObterMateriasAsync(List<EntradaSitemapTVFoco> links)
        {
            var materias = new List<MateriaTVFoco>();

            foreach (EntradaSitemapTVFoco link in links)
            {
                MateriaTVFoco materia = await fonte.ObterMateriaAsync(link.Endereco).ConfigureAwait(false);

                if (materia != null)
                    materias.Add(ComDataDoSitemap(materia, link));

                await Task.Delay(PAUSA_ENTRE_MATERIAS_MS).ConfigureAwait(false);
            }

            return materias;
        }

        private static MateriaTVFoco ComDataDoSitemap(MateriaTVFoco materia, EntradaSitemapTVFoco entrada)
        {
            if (entrada.Publicacao == DateTime.MinValue)
                return materia;

            return new MateriaTVFoco(materia.Titulo, materia.Autor, materia.Imagem, entrada.Publicacao);
        }

        private static KeyValuePair<string, RssRecoverType> SemConteudo()
        {
            return new KeyValuePair<string, RssRecoverType>(EscritorFeedTVFocoXml.Escrever(new List<MateriaTVFoco>()), RssRecoverType.SemConteudo);
        }
    }
}
