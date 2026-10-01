using NLog;
using System;
using System.Net;
using System.Threading.Tasks;
using TVPlayerAPI.Helpers;

namespace TVPlayerAPI.UOL.Fontes
{
    internal sealed class FonteHttp : IFonteHtml
    {
        private const int MAX_TENTATIVAS = 2;
        private const int ESPERA_TENTATIVA_MS = 1500;
        private const int DNS_REFRESH_MS = 30000;

        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public string Nome
        {
            get { return "http"; }
        }

        public async Task<string> ObterHtmlAsync(string url)
        {
            for (int tentativa = 1; tentativa <= MAX_TENTATIVAS; tentativa++)
            {
                string html = await WebHelper.GetJSONAsync(url).ConfigureAwait(false);

                if (!string.IsNullOrEmpty(html))
                    return html;

                if (tentativa < MAX_TENTATIVAS)
                {
                    RenovarConexao(url);
                    await Task.Delay(ESPERA_TENTATIVA_MS * tentativa).ConfigureAwait(false);
                }
            }

            return string.Empty;
        }

        private static void RenovarConexao(string url)
        {
            try
            {
                ServicePointManager.DnsRefreshTimeout = DNS_REFRESH_MS;

                ServicePoint ponto = ServicePointManager.FindServicePoint(new Uri(url));
                ponto.ConnectionLeaseTimeout = 0;
                ponto.CloseConnectionGroup(string.Empty);
            }
            catch (Exception ex)
            {
                logger.Debug(ex, "UOL: nao foi possivel renovar a conexao para " + url);
            }
        }
    }
}
