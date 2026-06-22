using Cardápio.Dto;
using Cardápio.Infra.Model;

namespace Cardápio.Infra.Interfaces.Repositories
{
    public interface IGrupoRepo : IRepo<Group>
    {
        Task<List<GrupoGetDTO>> GetGrupoByUsuarioIDAsync(int usuarioID);
        Task<GrupoGetDTO> GetGrupoByIDAsync(int grupoID);
        Task<Group> GetGrupoModelByIDAsync(int grupoID);
        Task<IEnumerable<GrupoGetDTO>> GetAllGrupoAsync();
    }
}
