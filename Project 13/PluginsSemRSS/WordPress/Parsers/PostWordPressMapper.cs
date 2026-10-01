using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using TVPlayerAPI.Helpers;
using TVPlayerAPI.WordPress.Classes;
using TVPlayerAPI.WordPress.ResponseWrappers;

namespace TVPlayerAPI.WordPress.Parsers
{
    internal static class PostWordPressMapper
    {
        private const int LARGURA_MAXIMA_DO_ORIGINAL = 2560;

        private const string VARIANTE_JETPACK_1920 = "?w=1920";

        private static readonly TimeSpan TOLERANCIA_PARA_DATA_FUTURA = TimeSpan.FromHours(1);

        internal static IReadOnlyList<NoticiaWordPress> Mapear(SiteWordPress site, IEnumerable<PostWordPress> posts)
        {
            if (posts == null)
                return new List<NoticiaWordPress>();

            return posts.Select(post => Mapear(site, post))
                        .Where(noticia => noticia != null)
                        .ToList();
        }

        internal static NoticiaWordPress Mapear(SiteWordPress site, PostWordPress post)
        {
            if (post == null)
                return null;

            string manchete = TextoLimpo(post.title?.rendered);
            string link = Endereco(post.link);
            DateTime? publicacao = DataDePublicacao(post.date_gmt);

            if (manchete == null || link == null || publicacao == null || EstaNoFuturo(publicacao.Value))
                return null;

            return new NoticiaWordPress(manchete, TextoLimpo(post.excerpt?.rendered), link, Imagem(site, post), publicacao.Value);
        }

        private static string Imagem(SiteWordPress site, PostWordPress post)
        {
            ImagemOpenGraphWordPress destaque = post.yoast_head_json?.og_image?.FirstOrDefault(imagem => Endereco(imagem?.url) != null);

            if (destaque == null)
                return Endereco(SemParametros(post.jetpack_featured_media_url));

            string original = SemParametros(destaque.url);

            if (site.RedimensionaPeloJetpack && destaque.width > LARGURA_MAXIMA_DO_ORIGINAL)
                return original + VARIANTE_JETPACK_1920;

            return original;
        }

        private static bool EstaNoFuturo(DateTime publicacao)
        {
            return publicacao > DateTime.UtcNow.Add(TOLERANCIA_PARA_DATA_FUTURA);
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

            string texto = RSSHelper.RemoveHTMLTags(valor);

            return texto.Length == 0 ? null : texto;
        }

        private static string SemParametros(string valor)
        {
            if (string.IsNullOrEmpty(valor))
                return null;

            int inicioDosParametros = valor.IndexOf('?');

            return (inicioDosParametros < 0 ? valor : valor.Substring(0, inicioDosParametros)).Trim();
        }

        private static string Endereco(string valor)
        {
            string endereco = string.IsNullOrEmpty(valor) ? null : valor.Trim();

            return Uri.IsWellFormedUriString(endereco, UriKind.Absolute) ? endereco : null;
        }
    }
}
