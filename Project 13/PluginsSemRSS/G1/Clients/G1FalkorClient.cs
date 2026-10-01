using NLog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using TVPlayerAPI.G1.Classes;
using TVPlayerAPI.G1.Interfaces;
using TVPlayerAPI.G1.Parsers;
using TVPlayerAPI.G1.ResponseWrappers;
using TVPlayerAPI.Helpers;

namespace TVPlayerAPI.G1.Clients
{
    internal class G1FalkorClient : IFonteNoticiaG1
    {
        private const string URL_SECAO = "https://g1.globo.com/{0}/";

        private const string URL_POSTS = "https://falkor-cda.bastian.globo.com/tenants/g1/instances/{0}/posts/page/{1}";

        private const int MAX_PAGINAS = 4;

        private static readonly ConcurrentDictionary<string, string> instanciasPorCaminho = new ConcurrentDictionary<string, string>();

        private readonly HttpDedicado http;

        private readonly Logger logger = LogManager.GetCurrentClassLogger();

        internal G1FalkorClient(HttpDedicado http)
        {
            this.http = http;
        }

        public string Nome
        {
            get { return "falkor"; }
        }

        public async Task<IReadOnlyList<NoticiaG1>> ObterNoticiasAsync(string caminhoDaSecao, int quantidade)
        {
            string instancia = await InstanciaAsync(caminhoDaSecao).ConfigureAwait(false);

            if (instancia == null)
                return new List<NoticiaG1>();

            var noticias = new List<NoticiaG1>();

            for (int pagina = 1; pagina <= MAX_PAGINAS && noticias.Count < quantidade; pagina++)
            {
                string url = string.Format(CultureInfo.InvariantCulture, URL_POSTS, instancia, pagina);
                FalkorResponse resposta = await http.ObterEDesserializarAsync<FalkorResponse>(url).ConfigureAwait(false);

                if (resposta == null || !resposta.TemConteudo)
                {
                    if (pagina == 1)
                        EsquecerInstancia(caminhoDaSecao, url);

                    break;
                }

                foreach (NoticiaG1 nova in FalkorPostMapper.Mapear(resposta.items))
                {
                    if (!noticias.Any(ja => MesmoLink(ja, nova)))
                        noticias.Add(nova);
                }
            }

            return noticias.Take(quantidade).ToList();
        }

        private static bool MesmoLink(NoticiaG1 uma, NoticiaG1 outra)
        {
            return string.Equals(uma.Link, outra.Link, StringComparison.OrdinalIgnoreCase);
        }

        private async Task<string> InstanciaAsync(string caminhoDaSecao)
        {
            if (instanciasPorCaminho.TryGetValue(caminhoDaSecao, out string conhecida))
                return conhecida;

            string urlDaSecao = string.Format(CultureInfo.InvariantCulture, URL_SECAO, caminhoDaSecao);
            string instancia = FalkorInstanciaParser.InstanciaDaPagina(await http.ObterTextoAsync(urlDaSecao).ConfigureAwait(false));

            if (instancia == null)
            {
                logger.Warn("G1: a pagina " + urlDaSecao + " nao cita instancia da api \"" + Nome + "\".");
                return null;
            }

            instanciasPorCaminho[caminhoDaSecao] = instancia;
            return instancia;
        }

        private void EsquecerInstancia(string caminhoDaSecao, string url)
        {
            instanciasPorCaminho.TryRemove(caminhoDaSecao, out string _);
            logger.Warn("G1: a api \"" + Nome + "\" nao devolveu conteudo em " + url + "; a instancia de " + caminhoDaSecao + " sera descoberta de novo.");
        }
    }
}
