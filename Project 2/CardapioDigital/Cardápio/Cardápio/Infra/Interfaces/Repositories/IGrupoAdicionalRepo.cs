using Cardápio.Infra.Model;
using Cardápio.Dto;

namespace Cardápio.Infra.Interfaces.Repositories
{
    public interface IGrupoAdicionalRepo : IRepo<GrupoAdicional>
    {
        public Task<GrupoAdicional> GetGrupoAdicionalAsyncByIdAndEmpresaID(int grupoID, int empresaID);
        public Task<GrupoAdicional> CompararGrupoEBuscarOuCriarNovo(int empresaID, GrupoAdicional groupDB);
        public Task<string> GetUniqueGroupName(string baseName, int empresaID,  int? grupoID = null);
    }
}
