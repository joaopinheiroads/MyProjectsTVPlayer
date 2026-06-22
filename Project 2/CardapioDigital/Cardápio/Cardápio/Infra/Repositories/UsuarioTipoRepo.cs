using Cardápio.Dto;
using Cardápio.Infra.Data;
using Cardápio.Infra.Interfaces.Repositories;
using Cardápio.Infra.Model;
using Microsoft.EntityFrameworkCore;

namespace Cardápio.Infra.Repositories
{
    public class UsuarioTipoRepo : IUsuarioTipoRepo
    {
        private readonly AppDbContext context;

        public UsuarioTipoRepo(AppDbContext context)
        {
            this.context = context;
        }
        public async Task AddAsync(UsuarioTipo entity)
        {
            try
            {
                if (entity == null)
                {
                    throw new ArgumentNullException(nameof(entity));
                }
                await context.UsuarioTipo.AddAsync(entity);
            }
            catch
            {
                throw;
            }
        }

        public async Task<IEnumerable<UsuarioTipoGetDTO>> GetUsuarioTipoByEmpresaIDAsync(int empresaID)
        {
            return await (from usuarioTipo in context.UsuarioTipo
                          where usuarioTipo.Ativo == true
                          select new UsuarioTipoGetDTO
                          {
                              ID = usuarioTipo.ID,
                              Nome = usuarioTipo.Nome
                          }).ToListAsync();
        }

        public async Task<UsuarioTipoGetDTO> GetUsuarioTipoByIDAsync(int usuarioTipoID, int empresaID)
        {
            return await (from usuarioTipo in context.UsuarioTipo
                          where usuarioTipo.Ativo == true
                          select new UsuarioTipoGetDTO
                          {
                              ID = usuarioTipo.ID,
                              Nome = usuarioTipo.Nome
                          }).FirstOrDefaultAsync();
        }

        public async Task<UsuarioTipo> GetByIDAsync(int usuarioTipoID, int empresaID)
        {
            return await (from usuarioTipo in context.UsuarioTipo
                          where usuarioTipo.ID == usuarioTipoID &&
                                usuarioTipo.Ativo == true
                          select usuarioTipo
                        ).FirstOrDefaultAsync();
        }

        public async Task UpdateAsync(UsuarioTipo entity)
        {
            await Task.FromResult(0);
        }
    }
}
