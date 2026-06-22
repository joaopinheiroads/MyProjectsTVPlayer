using Cardápio.Dto;
using Cardápio.Infra.Model;

namespace Cardápio.Infra.Interfaces.Repositories
{
    public interface IPedidoRepo : IRepo<Pedido>
    {
        public Task<IEnumerable<TodosPedidoGetDTO>> GetDTOByEmpresaID(int empresaID);
    }
}
