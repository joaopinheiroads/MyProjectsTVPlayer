using Cardápio.Dto;
using Cardápio.Infra.Model;

namespace Cardápio.Infra.Interfaces.Repositories
{
    public interface IQrCodeLayoutRepo : IRepo<QrCodeLayout>
    {
        public Task<IEnumerable<QrCodeLayoutGetDTO>> GetByEmpresaIDAsync(int empresaID);
    }
}
