using System.Collections.Generic;
using System.Linq;

namespace TVPlayerAPI.G1.Classes
{
    internal class G1Secao
    {
        private const string CANAL_GERAL = "g1";

        internal string Chave { get; private set; }

        internal string Canal { get; private set; }

        internal IReadOnlyList<string> Caminhos { get; private set; }

        private G1Secao(string chave, string canal, params string[] caminhos)
        {
            Chave = chave;
            Canal = canal;
            Caminhos = caminhos;
        }

        internal static readonly IReadOnlyList<G1Secao> Conhecidas = new List<G1Secao>
        {
            new G1Secao(string.Empty, CANAL_GERAL, "ultimas-noticias"),
            new G1Secao("economia", "g1 > Economia", "economia"),
            new G1Secao("mundo", "g1 > Mundo", "mundo"),
            new G1Secao("carros", "g1 > Carros", "carros"),
            new G1Secao("natureza", "g1 > Meio Ambiente", "meio-ambiente"),
            new G1Secao("ciencia-e-saude", "g1 > Ciência e Saúde", "ciencia", "saude"),
            new G1Secao("concursos-e-emprego", "g1 > Trabalho e Carreira", "trabalho-e-carreira"),
            new G1Secao("sp/vale-do-paraiba-regiao", "g1 > Vale do Paraíba e Região", "sp/vale-do-paraiba-regiao"),
            new G1Secao("sp/santos-regiao", "g1 > Santos e Região", "sp/santos-regiao"),
            new G1Secao("sao-paulo/sao-jose-do-rio-preto-aracatuba", "g1 > Rio Preto e Araçatuba", "sp/sao-jose-do-rio-preto-aracatuba")
        };

        internal static G1Secao Para(string chave)
        {
            return Conhecidas.FirstOrDefault(secao => secao.Chave == chave)
                   ?? new G1Secao(chave, CANAL_GERAL, chave);
        }
    }
}
