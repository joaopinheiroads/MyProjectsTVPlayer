using Cardápio.Infra.Data;
using Cardápio.Infra.Interfaces.Repositories;
using Cardápio.Infra.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Cardápio.Infra.Repositories
{
    public class GrupoUsuarioRepo : IGrupoUsuarioRepo
    {
        private readonly AppDbContext context;

        public GrupoUsuarioRepo(AppDbContext context)
        {
            this.context = context;
        }

        public async Task<UserGroup> GetGroupByUserID(int usuarioID)
        {
            return await (from grupoUsuario in context.GrupoUsuario
                          where grupoUsuario.UsuarioID == usuarioID &&
                          grupoUsuario.Ativo == true
                          select grupoUsuario).FirstOrDefaultAsync();
        }

        public async Task AddAsync(UserGroup grupoUsuario)
        {
            try
            {
                if (grupoUsuario == null)
                {
                    throw new ArgumentNullException(nameof(grupoUsuario));
                }
                await context.GrupoUsuario.AddAsync(grupoUsuario);
            }
            catch
            {
                throw;
            }
        }

        public async Task UpdateAsync(UserGroup grupoUsuario)
        {
            grupoUsuario.DataEdicao = DateTime.Now;
            await Task.FromResult(0);
        }

        public async Task<UserGroup> GetByIDAsync(int grupoUsuarioID, int empresaID)
        {
            return await (from grupoUsuario in context.GrupoUsuario
                          where grupoUsuario.ID == grupoUsuarioID &&
                                grupoUsuario.Ativo == true
                          select grupoUsuario
                        ).FirstOrDefaultAsync();
        }
    }
}
