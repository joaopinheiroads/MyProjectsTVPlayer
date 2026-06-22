using Cardápio.Dto;
using Cardápio.Infra.Model;

namespace Cardápio.Infra.Interfaces.Repositories
{
    public interface IUsuarioClienteRepo : IRepo<UsuarioCliente>
    {
        public Task<UsuarioCliente> GetByEmail(string email);

        public Task<UsuarioCliente> GetByPhone(string phone);

        public Task<UsuarioClienteGetDTO> GetByIDAsyncDTO(int usuarioClienteID);
        public Task<UsuarioCliente> GetByIDAsync(int usuarioClienteID);
    }
}
