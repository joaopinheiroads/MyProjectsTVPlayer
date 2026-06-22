using Cardápio.Dto;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cardápio.Infra.Interfaces.Repositories
{
    public interface IEstadoRepo
    {
        Task<IEnumerable<EstadoGetDTO>> GetEstadoByEmpresaIDAsync(int empresaID);
        Task<EstadoGetDTO> GetEstadoByIDAsync(string estadoID, int empresaID);
    }
}
