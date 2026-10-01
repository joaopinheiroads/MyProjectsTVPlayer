using CefSharp;
using CefSharp.OffScreen;
using NLog;
using System;
using System.Threading.Tasks;
using TVPlayerAPI.UOL.Fontes;

namespace TI.TVPlayer.Service.Util
{
    public sealed class NavegadorCef : IFonteHtml
    {
        private const int ESPERA_INICIALIZACAO_MS = 20000;
        private const int PASSO_MS = 250;
        private const int TIMEOUT_NAVEGACAO_MS = 45000;
        private const int STATUS_OK = 200;

        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private readonly FiltroDeRecursos filtro = new FiltroDeRecursos();

        private ChromiumWebBrowser navegador;

        public string Nome
        {
            get { return "navegador"; }
        }

        public async Task<string> ObterHtmlAsync(string url)
        {
            using (await CefCompartilhado.ReservarAsync().ConfigureAwait(false))
            {
                try
                {
                    await GarantirNavegadorAsync().ConfigureAwait(false);

                    filtro.EsquecerStatus();

                    if (!await CarregarAsync(url).ConfigureAwait(false))
                    {
                        logger.Warn("UOL: navegador estourou " + (TIMEOUT_NAVEGACAO_MS / 1000) + "s em " + url);
                        return string.Empty;
                    }

                    if (filtro.UltimoStatus != STATUS_OK)
                    {
                        logger.Warn("UOL: navegador recebeu status " + filtro.UltimoStatus + " em " + url);
                        return string.Empty;
                    }

                    return await navegador.GetSourceAsync().ConfigureAwait(false) ?? string.Empty;
                }
                catch (Exception ex)
                {
                    logger.Warn(ex, "UOL: navegador falhou em " + url);
                    return string.Empty;
                }
            }
        }

        private async Task GarantirNavegadorAsync()
        {
            if (navegador != null && navegador.IsBrowserInitialized)
                return;

            CefCompartilhado.Inicializar();

            navegador = new ChromiumWebBrowser();
            navegador.RequestHandler = filtro;

            for (int espera = 0; espera < ESPERA_INICIALIZACAO_MS && !navegador.IsBrowserInitialized; espera += PASSO_MS)
                await Task.Delay(PASSO_MS).ConfigureAwait(false);

            if (!navegador.IsBrowserInitialized)
                throw new InvalidOperationException("O navegador CEF nao inicializou em " + (ESPERA_INICIALIZACAO_MS / 1000) + "s.");
        }

        private async Task<bool> CarregarAsync(string url)
        {
            var conclusao = new TaskCompletionSource<bool>();

            EventHandler<LoadingStateChangedEventArgs> aoMudarEstado = null;

            aoMudarEstado = (remetente, evento) =>
            {
                if (evento.IsLoading)
                    return;

                conclusao.TrySetResult(true);
            };

            navegador.LoadingStateChanged += aoMudarEstado;

            try
            {
                navegador.Load(url);

                Task terminou = await Task.WhenAny(conclusao.Task, Task.Delay(TIMEOUT_NAVEGACAO_MS)).ConfigureAwait(false);

                return terminou == conclusao.Task;
            }
            finally
            {
                navegador.LoadingStateChanged -= aoMudarEstado;
            }
        }
    }

    internal sealed class FiltroDeRecursos : CefSharp.Handler.DefaultRequestHandler
    {
        private static readonly ResourceType[] DESCARTAVEIS =
        {
            ResourceType.Image,
            ResourceType.Stylesheet,
            ResourceType.FontResource,
            ResourceType.Media
        };

        internal int UltimoStatus { get; private set; }

        internal void EsquecerStatus()
        {
            UltimoStatus = 0;
        }

        public override CefReturnValue OnBeforeResourceLoad(IWebBrowser browserControl, IBrowser browser,
            IFrame frame, IRequest request, IRequestCallback callback)
        {
            return Array.IndexOf(DESCARTAVEIS, request.ResourceType) >= 0
                ? CefReturnValue.Cancel
                : CefReturnValue.Continue;
        }

        public override void OnResourceLoadComplete(IWebBrowser browserControl, IBrowser browser,
            IFrame frame, IRequest request, IResponse response, UrlRequestStatus status,
            long receivedContentLength)
        {
            if (request.ResourceType == ResourceType.MainFrame)
                UltimoStatus = response.StatusCode;
        }
    }
}
