using Cardápio.Dto;
using Cardápio.Infra.Model;

namespace Cardápio.Infra.Interfaces.Repositories
{
    public interface IProdutoGrupoAdicionalRepo : IRepo<ProdutoGrupoAdicional>
    {
        public Task<IEnumerable<ProdutoGrupoAdicional>> GetByProductIDAsync(int productID);
        public Task<ProdutoGrupoAdicional> GetByIDAndProductID(int grupoID, int produtoID);
        public Task<int> GetNextOrdemByProdutoGrupoAdicionalAsync(int produtoID, int grupoAdicionalID);
    }
}
