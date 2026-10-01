using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using TVPlayerAPI.Helpers;
using TVPlayerAPI.IG.Classes;
using TVPlayerAPI.IG.ResponseWrappers;

namespace TVPlayerAPI.IG.Parsers
{
    internal static class NoticiaIGMapper
    {
        private const string FORMATO_PUBDATE_RFC822 = "ddd, dd MMM yyyy HH:mm:ss";

        private const string FUSO_NUMERICO_UTC = " +0000";

        internal static IReadOnlyList<NoticiaIG> Mapear(IEnumerable<ContentNewsDoc> documentos)
        {
            if (documentos == null)
                return new List<NoticiaIG>();

            return documentos.Select(Mapear)
                             .Where(noticia => noticia != null)
                             .ToList();
        }

        internal static NoticiaIG Mapear(ContentNewsDoc documento)
        {
            if (documento == null)
                return null;

            string manchete = TextoLimpo(documento.titulo);
            string imagem = Endereco(documento.urlImgEmp_1200x675);
            string publicacao = PubDateRfc822(documento.startDate);

            if (manchete == null || imagem == null || publicacao == null)
                return null;

            return new NoticiaIG(manchete, imagem, publicacao);
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

        private static string PubDateRfc822(string valor)
        {
            DateTimeOffset quando;

            if (!DateTimeOffset.TryParse(valor, CultureInfo.InvariantCulture,
                                         DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out quando))
                return null;

            return quando.UtcDateTime.ToString(FORMATO_PUBDATE_RFC822, CultureInfo.InvariantCulture) + FUSO_NUMERICO_UTC;
        }
    }
}
