using System;

namespace TVPlayerAPI.UOL.Cotacoes.Classes
{
    internal class CotacaoUOL
    {
        internal decimal? Compra { get; private set; }

        internal decimal Valor { get; private set; }

        internal decimal VariacaoPercentual { get; private set; }

        internal DateTime MomentoUtc { get; private set; }

        internal CotacaoUOL(decimal? compra, decimal valor, decimal variacaoPercentual, DateTime momentoUtc)
        {
            Compra = compra;
            Valor = valor;
            VariacaoPercentual = variacaoPercentual;
            MomentoUtc = momentoUtc;
        }
    }
}
