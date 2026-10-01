using System.Threading.Tasks;
using TVPlayerAPI.UOL.Cotacoes.Classes;

namespace TVPlayerAPI.UOL.Cotacoes.Interfaces
{
    internal interface IFonteCotacoesUOL
    {
        string Nome { get; }

        Task<CotacaoUOL> ObterMoedaAsync(int idMoeda);

        Task<CotacaoUOL> ObterIndiceAsync(int idIndice);
    }
}
