using System.Threading.Tasks;
using TVPlayerAPI.G1.Classes;
using TVPlayerAPI.G1.Interfaces;
using TVPlayerAPI.Helpers;

namespace TVPlayerAPI.G1.Clients
{
    internal class G1MateriaClient : IFonteMateriaG1
    {
        private const string PROPRIEDADE_IMAGEM = "og:image";

        private const string PROPRIEDADE_RESUMO = "og:description";

        private readonly HttpDedicado http;

        internal G1MateriaClient(HttpDedicado http)
        {
            this.http = http;
        }

        public async Task<MateriaG1> ObterMateriaAsync(string linkDaMateria)
        {
            string html = await http.ObterTextoAsync(linkDaMateria).ConfigureAwait(false);

            return new MateriaG1(LeitorOpenGraph.Valor(html, PROPRIEDADE_IMAGEM),
                                 LeitorOpenGraph.Valor(html, PROPRIEDADE_RESUMO));
        }
    }
}
