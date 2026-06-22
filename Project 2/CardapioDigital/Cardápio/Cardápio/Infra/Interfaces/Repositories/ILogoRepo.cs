using Cardápio.Infra.Model;

namespace Cardápio.Infra.Interfaces.Repositories
{
    public interface ILogoRepo : IRepo<Logo>
    {
        Task<Logo> GetByCompanyIDAsync(int empresaID);
    }
}
