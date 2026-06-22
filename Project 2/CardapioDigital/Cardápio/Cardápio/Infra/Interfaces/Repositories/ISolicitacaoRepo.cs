using Cardápio.Infra.Model;

namespace Cardápio.Infra.Interfaces.Repositories
{
    public interface ISolicitacaoRepo : IRepo<SolicitacaoDemonstracao>
    {
        public Task<SolicitacaoDemonstracao> GetBySolicitIDAsync(Guid SolicitID);
    }
}
