using Cardápio.Dto;
using Cardápio.Infra.Model;

namespace Cardápio.Infra.Interfaces.Repositories
{
    public interface IUsuarioRepo : IRepo<User>
    {
        Task<IEnumerable<UsuarioGetDTO>> GetUsuarioAllAsync();
        Task<IEnumerable<UsuarioGetDTO>> GetAllUsuarioByGroupIDAsync(int groupID, int userID, bool isAdmin);
        Task<IEnumerable<UsuarioGetDTO>> GetUsuarioByUsuarioIDAsync(int usuarioID);
        Task<IEnumerable<UsuarioGetDTO>> GetUsuarioByEmpresaIDAsync(int empresaID);
        Task<UsuarioGetDTO> GetUsuarioByIDAsync(int usuarioID);
        Task<UsuarioGetDTO> GetUsuarioAuthAsync(string email, string password);
        Task<IEnumerable<User>> GetAllUserByCompanyIDNonAdmin(int empresaID);
        Task<IEnumerable<User>> GetAllUserByCompanyID(int empresaID);
        Task<User> GetUsuarioAdminByGrupoId(int groupID);
        Task<User> GetByEmailAsync(string email);
    }
}
