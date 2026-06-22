using Cardápio.Dto;
using Cardápio.Infra.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cardápio.Infra.Interfaces.Repositories
{
    public interface IUsuarioTipoRepo : IRepo<UsuarioTipo>
    {
        Task<IEnumerable<UsuarioTipoGetDTO>> GetUsuarioTipoByEmpresaIDAsync(int empresaID);
        Task<UsuarioTipoGetDTO> GetUsuarioTipoByIDAsync(int usuarioTipoID, int empresaID);
    }
}
