using CefSharp;
using CefSharp.OffScreen;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace TI.TVPlayer.Service.Util
{
    internal static class CefCompartilhado
    {
        private static readonly SemaphoreSlim exclusividade = new SemaphoreSlim(1, 1);
        private static readonly object trava = new object();

        internal static void Inicializar()
        {
            lock (trava)
            {
                if (Cef.IsInitialized == true)
                    return;

                var configuracao = new CefSettings
                {
                    CachePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CefSharp", "Cache"),
                    LogSeverity = LogSeverity.Disable
                };

                Cef.Initialize(cefSettings: configuracao, performDependencyCheck: true, browserProcessHandler: null);
            }
        }

        internal static async Task<IDisposable> ReservarAsync()
        {
            await exclusividade.WaitAsync().ConfigureAwait(false);
            return new Reserva(exclusividade);
        }

        private sealed class Reserva : IDisposable
        {
            private readonly SemaphoreSlim origem;

            private bool liberada;

            internal Reserva(SemaphoreSlim origem)
            {
                this.origem = origem;
            }

            public void Dispose()
            {
                if (liberada)
                    return;

                liberada = true;
                origem.Release();
            }
        }
    }
}
