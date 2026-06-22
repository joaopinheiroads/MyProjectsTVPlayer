using Cardápio.Dto;
using Cardápio.Infra.Model;

namespace Cardápio.Infra.Interfaces.Repositories
{
    public interface ICategoriaRepo : IRepo<Category>
    {
        Task<IEnumerable<CategoriaGetDTO>> GetCategoriaByEmpresaIDAsync(int empresaID, int usuarioID, int grupoID, bool isAdmin);
        Task<IEnumerable<CategoriaGetDTO>> GetCategoriaByEmpresaIDAsyncNotAuthenticated(int empresaID);
        //duplicado
        Task<IEnumerable<CategoriaGetDTO>> GetCategoriaByEmpresaIDAsyncNotAuthenticated(string empresaID);
        Task<CategoriaGetDTO> GetCategoriaByIDAsync(int categoriaID, int empresaID);

        Task<int> GetNextOrdemByEmpresaIDAsync(int empresaID);

        Task<ICollection<Category>> GetByListCategoriaIDAsync(List<int> listCategoriaID, int empresaID);
        Task<ICollection<Category>> GetByBiggerThenOrdemAsync(int ordem, int empresaID);
        Task UpdateRangeAsync(ICollection<Category> categoriasExistentes);
    }
}
