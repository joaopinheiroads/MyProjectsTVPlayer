using Cardápio.Infra.Data;
using Cardápio.Infra.Interfaces.Repositories;
using Cardápio.Infra.Model;
using Microsoft.EntityFrameworkCore;

namespace Cardápio.Infra.Repositories
{
    public class UsuarioStatusRepo : IUsuarioStatusRepo
    {
        private readonly AppDbContext context;

        public UsuarioStatusRepo(AppDbContext context)
        {
            this.context = context;
        }

        public async Task AddAsync(UsuarioStatus entity)
        {
            try
            {
                if (entity == null)
                {
                    throw new ArgumentNullException(nameof(entity));
                }
                await context.UsuarioStatus.AddAsync(entity);
            }
            catch
            {
                throw;
            }
        }

        public async Task<UsuarioStatus> GetByIDAsync(int usuarioTipoID, int empresaID)
        {
            return await (from usuarioStatus in context.UsuarioStatus
                          where usuarioStatus.ID == usuarioTipoID &&
                                usuarioStatus.Ativo == true
                          select usuarioStatus
                        ).FirstOrDefaultAsync();
        }

        public async Task UpdateAsync(UsuarioStatus entity)
        {
            await Task.FromResult(0);
        }
    }
}
