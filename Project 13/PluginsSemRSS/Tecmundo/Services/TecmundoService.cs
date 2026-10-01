using NLog;
using SerializableObjects.PluginsTVPlayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TVPlayerAPI.Helpers;
using TVPlayerAPI.Tecmundo.Classes;
using TVPlayerAPI.Tecmundo.Clients;
using TVPlayerAPI.Tecmundo.Interfaces;
using TVPlayerAPI.Tecmundo.Saida;

namespace TVPlayerAPI.Tecmundo.Services
{
    internal class TecmundoService
    {
        private const int MAX_ITENS = 15;
        private const int PAUSA_ENTRE_MATERIAS_MS = 200;
        private const string NOME_DO_CANAL = "Novidades do TecMundo";
        private const string HOST_DO_PLUGIN = "tecmundo.com.br";

        private static readonly HttpDedicado http = new HttpDedicado("Tecmundo");

        private readonly IFonteNoticiaTecmundo fonte;

        private readonly Logger logger = LogManager.GetCurrentClassLogger();

        internal TecmundoService(IFonteNoticiaTecmundo fonte)
        {
            this.fonte = fonte;
        }

        internal static bool Atende(string urlDoPlugin)
        {
            return Uri.TryCreate(urlDoPlugin?.Trim(), UriKind.Absolute, out Uri uri)
                && (uri.Host.Equals(HOST_DO_PLUGIN, StringComparison.OrdinalIgnoreCase)
                    || uri.Host.EndsWith("." + HOST_DO_PLUGIN, StringComparison.OrdinalIgnoreCase));
        }

        internal static Task<KeyValuePair<string, RssRecoverType>> RecuperarAsync()
        {
            return new TecmundoService(new EstadaoTecmundoClient(http)).ColetarAsync();
        }

        internal async Task<KeyValuePair<string, RssRecoverType>> ColetarAsync()
        {
            try
            {
                List<Uri> links = await LinksMaisRecentesAsync().ConfigureAwait(false);

                if (links.Count == 0)
                {
                    logger.Warn("Tecmundo: o " + fonte.Nome + " nao trouxe noticias do TecMundo; a pasta mantem o conteudo anterior.");
                    return SemConteudo();
                }

                List<MateriaTecmundo> materias = await ObterMateriasAsync(links).ConfigureAwait(false);

                if (materias.Count == 0)
                {
                    logger.Warn("Tecmundo: nenhuma das " + links.Count + " materias trouxe manchete e imagem; a pasta mantem o conteudo anterior.");
                    return SemConteudo();
                }

                logger.Info("Tecmundo: coletou " + materias.Count + " de " + links.Count + " materias do " + fonte.Nome + ".");

                List<MateriaTecmundo> maisNovasPrimeiro = materias.OrderByDescending(materia => materia.Publicacao).ToList();

                return new KeyValuePair<string, RssRecoverType>(EscritorFeedTecmundoXml.Escrever(NOME_DO_CANAL, maisNovasPrimeiro), RssRecoverType.Sucesso);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Tecmundo: falha ao coletar as noticias do Estadao.");
                return new KeyValuePair<string, RssRecoverType>(string.Empty, RssRecoverType.Erro);
            }
        }

        private async Task<List<Uri>> LinksMaisRecentesAsync()
        {
            var entradas = new List<EntradaSitemapTecmundo>();

            entradas.AddRange(await fonte.ObterEntradasDeHojeAsync().ConfigureAwait(false));

            if (MaisRecentesSemRepeticao(entradas).Count < MAX_ITENS)
                entradas.AddRange(await fonte.ObterEntradasDoDiaAsync(DateTime.UtcNow.Date.AddDays(-1)).ConfigureAwait(false));

            return MaisRecentesSemRepeticao(entradas).Take(MAX_ITENS).ToList();
        }

        private static List<Uri> MaisRecentesSemRepeticao(List<EntradaSitemapTecmundo> entradas)
        {
            return entradas
                .GroupBy(entrada => entrada.Endereco)
                .Select(grupo => new { Endereco = grupo.Key, Atualizacao = grupo.Max(entrada => entrada.Atualizacao) })
                .OrderByDescending(entrada => entrada.Atualizacao)
                .Select(entrada => entrada.Endereco)
                .ToList();
        }

        private async Task<List<MateriaTecmundo>> ObterMateriasAsync(List<Uri> links)
        {
            var materias = new List<MateriaTecmundo>();

            foreach (Uri link in links)
            {
                MateriaTecmundo materia = await fonte.ObterMateriaAsync(link).ConfigureAwait(false);

                if (materia != null)
                    materias.Add(materia);

                await Task.Delay(PAUSA_ENTRE_MATERIAS_MS).ConfigureAwait(false);
            }

            return materias;
        }

        private static KeyValuePair<string, RssRecoverType> SemConteudo()
        {
            return new KeyValuePair<string, RssRecoverType>(EscritorFeedTecmundoXml.Escrever(NOME_DO_CANAL, new List<MateriaTecmundo>()), RssRecoverType.SemConteudo);
        }
    }
}
