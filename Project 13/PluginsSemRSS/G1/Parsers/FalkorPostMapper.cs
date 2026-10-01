using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using TVPlayerAPI.G1.Classes;
using TVPlayerAPI.G1.ResponseWrappers;
using TVPlayerAPI.Helpers;

namespace TVPlayerAPI.G1.Parsers
{
    internal static class FalkorPostMapper
    {
        private const string TIPO_VIDEO = "video";

        private const string TAMANHO_DA_IMAGEM_RESERVA = "L";

        private static readonly TimeSpan TOLERANCIA_PARA_DATA_FUTURA = TimeSpan.FromHours(1);

        internal static IReadOnlyList<NoticiaG1> Mapear(IEnumerable<FalkorPost> posts)
        {
            if (posts == null)
                return new List<NoticiaG1>();

            return posts.Select(Mapear)
                        .Where(noticia => noticia != null)
                        .ToList();
        }

        internal static NoticiaG1 Mapear(FalkorPost post)
        {
            FalkorConteudo conteudo = post?.content;

            if (conteudo == null || EhVideo(post))
                return null;

            string manchete = TextoLimpo(conteudo.title);
            string link = Endereco(conteudo.url);
            DateTime? publicacao = DataDePublicacao(post.publication);

            if (manchete == null || link == null || publicacao == null || EstaNoFuturo(publicacao.Value))
                return null;

            return new NoticiaG1(manchete, TextoLimpo(conteudo.summary), link, ImagemReserva(conteudo.image), publicacao.Value);
        }

        private static bool EhVideo(FalkorPost post)
        {
            return string.Equals(post.type, TIPO_VIDEO, StringComparison.OrdinalIgnoreCase)
                   || string.Equals(post.content.type, TIPO_VIDEO, StringComparison.OrdinalIgnoreCase);
        }

        private static bool EstaNoFuturo(DateTime publicacao)
        {
            return publicacao > DateTime.UtcNow.Add(TOLERANCIA_PARA_DATA_FUTURA);
        }

        private static string ImagemReserva(FalkorImagem imagem)
        {
            if (imagem?.sizes == null || !imagem.sizes.TryGetValue(TAMANHO_DA_IMAGEM_RESERVA, out FalkorTamanho tamanho))
                return null;

            return Endereco(tamanho?.url);
        }

        private static DateTime? DataDePublicacao(string valor)
        {
            if (!DateTimeOffset.TryParse(valor, CultureInfo.InvariantCulture,
                                         DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out DateTimeOffset quando))
                return null;

            return quando.UtcDateTime;
        }

        private static string TextoLimpo(string valor)
        {
            if (string.IsNullOrEmpty(valor))
                return null;

            string texto = WebUtility.HtmlDecode(RSSHelper.RemoveHTMLTags(valor)).Trim();

            return texto.Length == 0 ? null : texto;
        }

        private static string Endereco(string valor)
        {
            string endereco = string.IsNullOrEmpty(valor) ? null : valor.Trim();

            return Uri.IsWellFormedUriString(endereco, UriKind.Absolute) ? endereco : null;
        }
    }
}
