using Cardápio.Dto;
using Cardápio.Infra.Model;

namespace Cardápio.Infra.Interfaces.Repositories
{
    public interface IEmpresaRepo : IRepo<Company>
    {
        Task<int> GetEmpresaIDAsync(string qrCode);
        Task<Company> GetByIDAsync(int empresaID);

        Task<EmpresaGetDTO> GetEmpresaByNameAsync(string empresaNome);
        Task<EmpresaGetDTO> GetEmpresaByIDAsync(int empresaID);

        Task<string> GetQRCodeByIDAsync(int empresaID);

        Task<IEnumerable<EmpresaGetDTO>> GetEnterprisesByUsuarioIDAsync(int usuarioID, int grupoID, int usuarioTipoID, int empresaID);
        Task<IEnumerable<EmpresaGetDTO>> GetEmpresaByUsuarioIDAsync(int usuarioID, int usuarioTipoID);
        Task<IEnumerable<EmpresaSelectDTO>> GetEmpresaByUsuarioIDSelectAsync(int usuarioID, int usuarioTipoID);
        Task<IEnumerable<EmpresaGetDTO>> GetEnterpriseByGrupoID(int grupoID);
        Task<IEnumerable<Company>> GetEnterprisesModelByGrupoID(int grupoID);
    }
}
