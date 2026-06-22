using System.Collections.Generic;
using System.Threading.Tasks;
using Cardápio.Dto;
using Cardápio.Infra.Model;

namespace Cardápio.Infra.Interfaces.Repositories
{
    public interface ILayoutRepo : IRepo<Layout>
    {
        Task<LayoutGetDTO> GetLayoutByIDAsync(int layoutID, int empresaID);
        Task<LayoutGetDTO> GetLayoutByEmpresaIDAsync(int empresaID);
    }
}
