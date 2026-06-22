using Cardápio.Dto;
using Cardápio.Infra.Model;

namespace Cardápio.Infra.Interfaces.Repositories
{
    public interface IGrupoAdicionalItemRepo : IRepo<GrupoAdicionalItem>
    {
        public Task<IEnumerable<GrupoAdicionalGetDTO>> GetAdditionalGroupByEmpresaID(int empresaID);
        public Task<IEnumerable<GrupoAdicionalGetDTO>> GetAdditionalGroupWithProductByEmpresaID(int empresaID);

        public Task<IEnumerable<GrupoAdicionalGetDTO>> GetAdditionalGroupByProdutoID(int productID);
    }
}
