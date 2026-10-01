using NLog;
using TVPlayerAPI.UOL.Classes;

namespace TVPlayerAPI.UOL.Fontes
{
    public static class FontesHtml
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private static readonly IFonteHtml http = new FonteHttp();

        private static IFonteHtml navegador;

        public static void RegistrarNavegador(IFonteHtml fonte)
        {
            if (fonte == null)
                return;

            navegador = fonte;
            logger.Info("UOL: navegador \"" + fonte.Nome + "\" registrado; as secoes bloqueadas passam por ele.");
        }

        internal static IFonteHtml Http
        {
            get { return http; }
        }

        internal static IFonteHtml Para(UOLSecao secao)
        {
            return secao.ExigeNavegador && navegador != null ? navegador : http;
        }
    }
}
