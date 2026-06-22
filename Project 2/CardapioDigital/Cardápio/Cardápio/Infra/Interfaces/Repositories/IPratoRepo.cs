using Cardápio.Dto;
using Cardápio.Infra.Model;

namespace Cardápio.Infra.Interfaces.Repositories
{
    public interface IProdutoRepo : IRepo<Product>
    {
        Task<IEnumerable<ProdutoGetDTO>> GetProdutoByQRCodeAsync(string qrCode);
        Task<IEnumerable<ProdutoGetDTO>> GetProdutoByEmpresaIdAsync(int empresaID, int usuarioID, int grupoID, bool isAdmin);
        Task<IEnumerable<ProdutoGetDTO>> GetProdutoByEmpresaNomeAsync(string empresaID);
        Task<IEnumerable<ProdutoGetDTO>> GetProdutoByGrupoID(int empresaID);
        Task<ProdutoGetDTO> GetProdutoByIDAsync(int produtoID, int empresaID);
        Task<IEnumerable<ProdutoGetDTO>> GetProdutoByGrupoAdicionalID(int grupoID);
    }
}
