using System.Collections.Generic;
using System.Linq;
using SerializableObjects.PluginsTVPlayer;

namespace TVPlayerAPI.UOL.Classes
{
    internal class UOLSecao
    {
        internal RSSType Tipo { get; private set; }

        internal string[] UrlsListagem { get; private set; }

        internal string HostEsperado { get; private set; }

        internal string CategoriaPadrao { get; private set; }

        internal bool ExigeNavegador { get; private set; }

        private UOLSecao(RSSType tipo, string hostEsperado, string categoriaPadrao, bool exigeNavegador, string[] urlsListagem)
        {
            Tipo = tipo;
            HostEsperado = hostEsperado;
            CategoriaPadrao = categoriaPadrao;
            ExigeNavegador = exigeNavegador;
            UrlsListagem = urlsListagem;
        }

        private UOLSecao(RSSType tipo, string hostEsperado, string categoriaPadrao, params string[] urlsListagem)
            : this(tipo, hostEsperado, categoriaPadrao, false, urlsListagem)
        {
        }

        private UOLSecao ComNavegador()
        {
            return new UOLSecao(Tipo, HostEsperado, CategoriaPadrao, true, UrlsListagem);
        }

        internal static readonly IReadOnlyList<UOLSecao> Ativas = new List<UOLSecao>
        {
            new UOLSecao(RSSType.UOLEsporte, "www.uol.com.br", "Esporte",
                "https://www.uol.com.br/esporte/ultimas/",
                "https://www.uol.com.br/esporte/"),

            new UOLSecao(RSSType.UOLEntretenimento, "www.uol.com.br", "Entretenimento",
                "https://www.uol.com.br/splash/ultimas/",
                "https://www.uol.com.br/splash/",
                "https://www.uol.com.br/splash/filmes/",
                "https://www.uol.com.br/splash/series/",
                "https://www.uol.com.br/splash/musica/"),

            new UOLSecao(RSSType.UOLCelebridades, "www.uol.com.br", "Celebridades",
                "https://www.uol.com.br/splash/celebs/"),

            new UOLSecao(RSSType.UOLTelevisao, "www.uol.com.br", "Televisao",
                "https://www.uol.com.br/splash/televisao/"),

            new UOLSecao(RSSType.UOLEconomia, "economia.uol.com.br", "Economia",
                "https://economia.uol.com.br/ultimas/",
                "https://economia.uol.com.br/empresas-e-negocios/",
                "https://economia.uol.com.br/dinheiro-e-renda/"),

            new UOLSecao(RSSType.UOLPolitica, "noticias.uol.com.br", "Politica",
                "https://noticias.uol.com.br/politica/").ComNavegador(),

            //PLUGIN modificado para que dentro da pasta de cotidiano contenha o conteúdo da categoria Saúde,
            //deve voltar cotidiano ao normal a partir da atualização 6.0 do player, contendo tanto a opção saúde, quanto, cotidiano

            new UOLSecao(RSSType.UOLCotidiano, "noticias.uol.com.br", "Saúde",
                "https://noticias.uol.com.br/saude/").ComNavegador(),

            new UOLSecao(RSSType.UOLInternacional, "noticias.uol.com.br", "Internacional",
                "https://noticias.uol.com.br/internacional/").ComNavegador()
        };

        internal static UOLSecao Por(RSSType tipo)
        {
            return Ativas.FirstOrDefault(s => s.Tipo == tipo);
        }
    }
}
