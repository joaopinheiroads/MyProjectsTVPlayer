using System.Collections.Generic;

namespace TVPlayerAPI.UOL.Cotacoes.Classes
{
    internal class IndicadorCotacao
    {
        internal static readonly IReadOnlyList<IndicadorCotacao> NaOrdemDaTela = new List<IndicadorCotacao>
        {
            new IndicadorCotacao("Dolar comercial", TipoIndicador.Moeda, 1),
            new IndicadorCotacao("Dolar paralelo", TipoIndicador.DolarParalelo),
            new IndicadorCotacao("Dolar turismo", TipoIndicador.Moeda, 3),
            new IndicadorCotacao("Euro", TipoIndicador.Moeda, 5),
            new IndicadorCotacao("Bovespa", TipoIndicador.Indice, 1),
            new IndicadorCotacao("Nasdaq", TipoIndicador.Descontinuado),
            new IndicadorCotacao("Japão", TipoIndicador.Descontinuado),
            new IndicadorCotacao("Londres", TipoIndicador.Descontinuado)
        };

        internal string NomeNaTela { get; private set; }

        internal TipoIndicador Tipo { get; private set; }

        internal int IdNaFonte { get; private set; }

        internal bool VemDaFonte
        {
            get { return Tipo == TipoIndicador.Moeda || Tipo == TipoIndicador.Indice; }
        }

        private IndicadorCotacao(string nomeNaTela, TipoIndicador tipo, int idNaFonte = 0)
        {
            NomeNaTela = nomeNaTela;
            Tipo = tipo;
            IdNaFonte = idNaFonte;
        }
    }
}
