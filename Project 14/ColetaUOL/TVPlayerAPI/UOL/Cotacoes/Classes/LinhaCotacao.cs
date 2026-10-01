using System;
using System.Globalization;

namespace TVPlayerAPI.UOL.Cotacoes.Classes
{
    internal class LinhaCotacao
    {
        private const string SEM_VALOR = "-";
        private const string ZERO_DO_PARALELO = "0";
        private const string VARIACAO_ZERADA = "0,00";
        private const string FORMATO_DUAS_CASAS = "0.00";

        private static readonly CultureInfo PORTUGUES = CultureInfo.GetCultureInfo("pt-BR");

        internal string Nome { get; private set; }

        internal string Compra { get; private set; }

        internal string Venda { get; private set; }

        internal string Variacao { get; private set; }

        internal DateTime PublicacaoUtc { get; private set; }

        private LinhaCotacao(string nome, string compra, string venda, string variacao, DateTime publicacaoUtc)
        {
            Nome = nome;
            Compra = compra;
            Venda = venda;
            Variacao = variacao;
            PublicacaoUtc = publicacaoUtc;
        }

        internal static LinhaCotacao DeMoeda(string nome, CotacaoUOL cotacao)
        {
            string compra = cotacao.Compra.HasValue ? DuasCasas(cotacao.Compra.Value) : SEM_VALOR;

            return new LinhaCotacao(nome, compra, DuasCasas(cotacao.Valor), DuasCasas(cotacao.VariacaoPercentual), cotacao.MomentoUtc);
        }

        internal static LinhaCotacao DeIndice(string nome, CotacaoUOL cotacao)
        {
            return new LinhaCotacao(nome, SEM_VALOR, null, DuasCasas(cotacao.VariacaoPercentual) + "%", cotacao.MomentoUtc);
        }

        internal static LinhaCotacao DolarParalelo(string nome)
        {
            return new LinhaCotacao(nome, ZERO_DO_PARALELO, ZERO_DO_PARALELO, ZERO_DO_PARALELO, DateTime.MaxValue);
        }

        internal static LinhaCotacao Descontinuada(string nome, DateTime publicacaoUtc)
        {
            return new LinhaCotacao(nome, SEM_VALOR, null, VARIACAO_ZERADA, publicacaoUtc);
        }

        private static string DuasCasas(decimal valor)
        {
            return Math.Round(valor, 2, MidpointRounding.AwayFromZero).ToString(FORMATO_DUAS_CASAS, PORTUGUES);
        }
    }
}
