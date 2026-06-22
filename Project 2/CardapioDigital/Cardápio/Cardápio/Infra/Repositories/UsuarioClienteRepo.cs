using Cardápio.Dto;
using Cardápio.Infra.Data;
using Cardápio.Infra.Interfaces.Repositories;
using Cardápio.Infra.Model;
using Microsoft.EntityFrameworkCore;

namespace Cardápio.Infra.Repositories
{
    public class UsuarioClienteRepo : IUsuarioClienteRepo
    {
        private readonly AppDbContext context;

        public UsuarioClienteRepo(AppDbContext context)
        {
            this.context = context;
        }

        public async Task AddAsync(UsuarioCliente entity)
        {
            try
            {
                if (entity == null)
                {
                    throw new ArgumentNullException(nameof(entity));
                }
                await context.UsuarioCliente.AddAsync(entity);
            }
            catch
            {
                throw;
            }
        }

        public async Task<UsuarioCliente> GetByIDAsync(int usuarioClienteID, int empresaID)
        {
            return await (from usuarioCliente in context.UsuarioCliente
                          where usuarioCliente.ID == usuarioClienteID &&
                                usuarioCliente.Ativo == true
                          select usuarioCliente
                        ).FirstOrDefaultAsync();
        }

        public async Task<UsuarioCliente> GetByIDAsync(int usuarioClienteID)
        {
            return await (from usuarioCliente in context.UsuarioCliente
                          where usuarioCliente.ID == usuarioClienteID &&
                                usuarioCliente.Ativo == true
                          select usuarioCliente
                        ).FirstOrDefaultAsync();
        }

        public async Task<UsuarioClienteGetDTO> GetByIDAsyncDTO(int usuarioClienteID)
        {
            return await (from usuarioCliente in context.UsuarioCliente
                          where usuarioCliente.ID == usuarioClienteID &&
                                usuarioCliente.Ativo == true
                          select new UsuarioClienteGetDTO
                          {
                              ID = usuarioCliente.ID,
                              Nome = usuarioCliente.Nome,
                              Email = usuarioCliente.Email,
                              Celular = usuarioCliente.Celular,
                              CPF = usuarioCliente.CPF,
                              UsuarioTipoID = usuarioCliente.UsuarioTipoID ?? 0
                          }
            ).FirstOrDefaultAsync();
        }

        public async Task<UsuarioCliente> GetByEmail(string email)
        {
            return await (from usuarioCliente in context.UsuarioCliente
                          where usuarioCliente.Email == email &&
                                usuarioCliente.Ativo == true
                          select usuarioCliente
                        ).FirstOrDefaultAsync();
        }

        public async Task<UsuarioCliente> GetByPhone(string phone)
        {
            return await (from usuarioCliente in context.UsuarioCliente
                          where usuarioCliente.Celular == phone &&
                                usuarioCliente.Ativo == true
                          select usuarioCliente
                        ).FirstOrDefaultAsync();
        }

        public async Task UpdateAsync(UsuarioCliente entity)
        {
            await Task.FromResult(0);
        }
    }
}
