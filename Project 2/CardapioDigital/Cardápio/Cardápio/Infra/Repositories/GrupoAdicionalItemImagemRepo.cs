using Cardápio.Infra.Data;
using Cardápio.Infra.Interfaces.Repositories;
using Cardápio.Infra.Model;
using Microsoft.EntityFrameworkCore;

namespace Cardápio.Infra.Repositories
{
    public class GrupoAdicionalItemImagemRepo : IGrupoAdicionalItemImagemRepo
    {
        private readonly AppDbContext context;

        public GrupoAdicionalItemImagemRepo(AppDbContext context)
        {
            this.context = context;
        }

        public async Task AddAsync(GrupoAdicionalItemImagem entity)
        {
            try
            {
                if (entity == null)
                {
                    throw new ArgumentNullException(nameof(entity));
                }
                await context.GrupoAdicionalItemImagem.AddAsync(entity);
            }
            catch
            {
                throw;
            }
        }

        public async Task<GrupoAdicionalItemImagem> GetByIDAsync(int grupoAdicionalItemImagemID, int empresaID)
        {
            return await (from grupoAdicionalItemImagem in context.GrupoAdicionalItemImagem
                          where grupoAdicionalItemImagem.ID == grupoAdicionalItemImagemID &&
                                grupoAdicionalItemImagem.Ativo == true
                          select grupoAdicionalItemImagem
                        ).FirstOrDefaultAsync();
        }

        public async Task UpdateAsync(GrupoAdicionalItemImagem grupoAdicionalItemImagem)
        {
            grupoAdicionalItemImagem.DataEdicao = DateTime.Now;
            await Task.FromResult(0);
        }
    }
}
