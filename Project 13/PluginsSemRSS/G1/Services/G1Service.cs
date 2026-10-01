using NLog;
using SerializableObjects.PluginsTVPlayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TVPlayerAPI.G1.Classes;
using TVPlayerAPI.G1.Clients;
using TVPlayerAPI.G1.Interfaces;
using TVPlayerAPI.G1.Parsers;
using TVPlayerAPI.G1.Saida;
using TVPlayerAPI.Helpers;

namespace TVPlayerAPI.G1.Services
{
    internal class G1Service
    {
        private const int MAX_ITENS = 15;

        private const int PAUSA_ENTRE_MATERIAS_MS = 200;

        private static readonly HttpDedicado http = new HttpDedicado("G1");

        private readonly IFonteNoticiaG1 fonte;

        private readonly IFonteMateriaG1 fonteMateria;

        private readonly Logger logger = LogManager.GetCurrentClassLogger();

        internal G1Service(IFonteNoticiaG1 fonte, IFonteMateriaG1 fonteMateria)
        {
            this.fonte = fonte;
            this.fonteMateria = fonteMateria;
        }

        internal static bool Atende(string urlDoPlugin)
        {
            return G1UrlDoPlugin.ChaveDaSecao(urlDoPlugin) != null;
        }

        internal static Task<KeyValuePair<string, RssRecoverType>> RecuperarAsync(string urlDoPlugin)
        {
            return new G1Service(new G1FalkorClient(http), new G1MateriaClient(http)).ColetarAsync(urlDoPlugin);
        }

        internal async Task<KeyValuePair<string, RssRecoverType>> ColetarAsync(string urlDoPlugin)
        {
            string chave = G1UrlDoPlugin.ChaveDaSecao(urlDoPlugin);

            if (chave == null)
                return new KeyValuePair<string, RssRecoverType>(string.Empty, RssRecoverType.NaoExiste);

            G1Secao secao = G1Secao.Para(chave);

            try
            {
                IReadOnlyList<NoticiaG1> maisNovas = await MaisNovasDaSecaoAsync(secao).ConfigureAwait(false);
                IReadOnlyList<NoticiaG1> completas = await CompletadasPelaMateriaAsync(maisNovas).ConfigureAwait(false);

                if (completas.Count == 0)
                {
                    logger.Warn("G1: " + secao.Canal + " (" + urlDoPlugin + ") nao trouxe noticia aproveitavel pela fonte \"" + fonte.Nome + "\"; a pasta mantem o conteudo anterior.");
                    return SemConteudo(secao);
                }

                logger.Info("G1: " + secao.Canal + " (" + urlDoPlugin + ") coletou " + completas.Count + " de " + MAX_ITENS + " noticias pela fonte \"" + fonte.Nome + "\".");

                return new KeyValuePair<string, RssRecoverType>(EscritorFeedG1Xml.Escrever(secao.Canal, completas), RssRecoverType.Sucesso);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "G1: falha ao coletar " + secao.Canal + " (" + urlDoPlugin + ") pela fonte \"" + fonte.Nome + "\".");
                return new KeyValuePair<string, RssRecoverType>(string.Empty, RssRecoverType.Erro);
            }
        }

        private async Task<IReadOnlyList<NoticiaG1>> MaisNovasDaSecaoAsync(G1Secao secao)
        {
            var todas = new List<NoticiaG1>();

            foreach (string caminho in secao.Caminhos)
                todas.AddRange(await fonte.ObterNoticiasAsync(caminho, MAX_ITENS).ConfigureAwait(false));

            return todas.GroupBy(noticia => noticia.Link, StringComparer.OrdinalIgnoreCase)
                        .Select(mesmoLink => mesmoLink.First())
                        .OrderByDescending(noticia => noticia.Publicacao)
                        .Take(MAX_ITENS)
                        .ToList();
        }

        private async Task<IReadOnlyList<NoticiaG1>> CompletadasPelaMateriaAsync(IReadOnlyList<NoticiaG1> noticias)
        {
            var completas = new List<NoticiaG1>();

            foreach (NoticiaG1 noticia in noticias)
            {
                MateriaG1 materia = await fonteMateria.ObterMateriaAsync(noticia.Link).ConfigureAwait(false);
                NoticiaG1 completada = noticia.CompletadaCom(materia);

                if (completada.Completa)
                    completas.Add(completada);
                else
                    logger.Warn("G1: materia sem imagem ou sem resumo ficou de fora: " + noticia.Link);

                await Task.Delay(PAUSA_ENTRE_MATERIAS_MS).ConfigureAwait(false);
            }

            return completas;
        }

        private static KeyValuePair<string, RssRecoverType> SemConteudo(G1Secao secao)
        {
            return new KeyValuePair<string, RssRecoverType>(EscritorFeedG1Xml.Escrever(secao.Canal, new List<NoticiaG1>()), RssRecoverType.SemConteudo);
        }
    }
}
