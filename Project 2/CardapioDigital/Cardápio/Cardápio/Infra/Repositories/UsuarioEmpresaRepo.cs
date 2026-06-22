using Cardápio.Infra.Data;
using Cardápio.Infra.Interfaces.Repositories;
using Cardápio.Infra.Model;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace Cardápio.Infra.Repositories
{
    public class UsuarioEmpresaRepo : IUsuarioEmpresaRepo
    {
        private readonly AppDbContext context;

        public UsuarioEmpresaRepo(AppDbContext context)
        {
            this.context = context;
        }

        public async Task AddAsync(UsuarioEmpresa entity)
        {
            try
            {
                if (entity == null)
                {
                    throw new ArgumentNullException(nameof(entity));
                }
                await context.UsuarioEmpresa.AddAsync(entity);
            }
            catch
            {
                throw;
            }
        }

        public async Task<UsuarioEmpresa> GetByIDAsync(int ID, int empresaID)
        {
            return await (from usuarioEmpresa in context.UsuarioEmpresa
                          where usuarioEmpresa.UsuarioID == ID && usuarioEmpresa.EmpresaID == empresaID
                          select usuarioEmpresa).FirstOrDefaultAsync();
        }

        public async Task UpdateAsync(UsuarioEmpresa entity)
        {
            await Task.FromResult(0);
        }
    }
}
