using Cardápio.Client.Pages.Grupo;
using Cardápio.Dto;
using Cardápio.Infra.Data;
using Cardápio.Infra.Interfaces.Repositories;
using Cardápio.Infra.Model;
using Microsoft.EntityFrameworkCore;

namespace Cardápio.Infra.Repositories
{
    public class ProdutoGrupoAdicionalRepo : IProdutoGrupoAdicionalRepo
    {
        private readonly AppDbContext context;

        public ProdutoGrupoAdicionalRepo(AppDbContext context)
        {
            this.context = context;
        }

        public async Task AddAsync(ProdutoGrupoAdicional entity)
        {
            try
            {
                if (entity == null)
                {
                    throw new ArgumentNullException(nameof(entity));
                }
                await context.ProdutoGrupoAdicional.AddAsync(entity);
            }
            catch
            {
                throw;
            }
        }

        public async Task<IEnumerable<ProdutoGrupoAdicional>> GetByProductIDAsync(int productID)
        {
            return await (from produtoGrupoAdicional in context.ProdutoGrupoAdicional
                          where produtoGrupoAdicional.ProdutoID == productID
                          select produtoGrupoAdicional).ToListAsync();
        }

        public async Task<ProdutoGrupoAdicional> GetByIDAndProductID(int grupoID, int produtoID)
        {
            return await (from produtoGrupoAdicional in context.ProdutoGrupoAdicional
                          where produtoGrupoAdicional.ProdutoID == produtoID && produtoGrupoAdicional.GrupoAdicionalID == grupoID
                          select produtoGrupoAdicional).FirstOrDefaultAsync();
        }

        public async Task<int> GetNextOrdemByProdutoGrupoAdicionalAsync(int produtoID, int grupoAdicionalID)
        {
            var result = await (from pg in context.ProdutoGrupoAdicional
                                where pg.ProdutoID == produtoID && pg.GrupoAdicional.Ativo
                                select pg.Posicao).ToListAsync();

            return result.DefaultIfEmpty(0).Max() + 1;
        }

        public async Task UpdateAsync(ProdutoGrupoAdicional produtoGrupo)
        {
            try
            {
                if (produtoGrupo == null)
                {
                    throw new ArgumentNullException(nameof(produtoGrupo));
                }
                
                context.ProdutoGrupoAdicional.Update(produtoGrupo);
                await Task.CompletedTask;
            }
            catch
            {
                throw;
            }
        }

        public async Task<ProdutoGrupoAdicional> GetByIDAsync(int produtoGrupoID, int empresaID)
        {
            return null;
        }
    }
}
