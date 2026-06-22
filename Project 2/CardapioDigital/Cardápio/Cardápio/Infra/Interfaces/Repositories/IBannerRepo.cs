using Cardápio.Infra.Model;

namespace Cardápio.Infra.Interfaces.Repositories
{
    public interface IBannerRepo : IRepo<Banner>
    {
        Task<Banner> GetByCompanyIDAsync(int empresaID);
    }
}
