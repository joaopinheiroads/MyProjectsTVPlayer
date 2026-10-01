using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace TVPlayerAPI.Helpers
{
    internal class HttpDedicado
    {
        private const int TIMEOUT_SEG = 300;

        private const string USER_AGENT = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/73.0.3683.105 Safari/537.36 Vivaldi/2.4.1488.40";
        private const string ACCEPT = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8";

        private readonly Logger logger;
        private readonly HttpClient cliente;
        private readonly string integracao;

        internal HttpDedicado(string integracao, IDictionary<string, string> cabecalhos = null)
        {
            this.integracao = integracao;
            this.logger = LogManager.GetLogger("TVPlayerAPI." + integracao + ".Http");

            var handler = new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            };

            cliente = new HttpClient(handler, true);
            cliente.Timeout = TimeSpan.FromSeconds(TIMEOUT_SEG);
            cliente.DefaultRequestHeaders.Add("User-Agent", USER_AGENT);
            cliente.DefaultRequestHeaders.Add("Accept", ACCEPT);

            if (cabecalhos != null)
            {
                foreach (KeyValuePair<string, string> cabecalho in cabecalhos)
                    cliente.DefaultRequestHeaders.Add(cabecalho.Key, cabecalho.Value);
            }
        }

        internal async Task<string> ObterTextoAsync(string url)
        {
            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                using (HttpResponseMessage resposta = await cliente.GetAsync(url).ConfigureAwait(false))
                {
                    resposta.EnsureSuccessStatusCode();

                    return await resposta.Content.ReadAsStringAsync().ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                logger.Warn(ex, integracao + ": falha ao obter " + url);
                return string.Empty;
            }
        }

        internal async Task<T> ObterEDesserializarAsync<T>(string url)
        {
            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                using (HttpResponseMessage resposta = await cliente.GetAsync(url).ConfigureAwait(false))
                {
                    resposta.EnsureSuccessStatusCode();

                    string json = await resposta.Content.ReadAsStringAsync().ConfigureAwait(false);

                    return JsonConvert.DeserializeObject<T>(json);
                }
            }
            catch (Exception ex)
            {
                logger.Warn(ex, integracao + ": falha ao obter " + url);
                return default(T);
            }
        }
    }
}
