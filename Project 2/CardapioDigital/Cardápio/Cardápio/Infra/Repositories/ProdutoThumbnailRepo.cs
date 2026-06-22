using Cardápio.Infra.Data;
using Cardápio.Infra.Interfaces.Repositories;
using Cardápio.Infra.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cardápio.Infra.Repositories
{
    public class ProdutoThumbnailRepo : IProdutoThumbnailRepo
    {
        private readonly AppDbContext context;

        public ProdutoThumbnailRepo(AppDbContext context)
        {
            this.context = context;
        }

        public async Task AddAsync(ThumbnailProduct entity)
        {
            try
            {
                if (entity == null)
                {
                    throw new ArgumentNullException(nameof(entity));
                }
                await context.ProdutoThumbnail.AddAsync(entity);
            }
            catch
            {
                throw;
            }
        }

        public async Task<ThumbnailProduct> GetThumbnailByID(int thumbnailID)
        {
            return await (from thumbnailProduct in context.ProdutoThumbnail
                          where thumbnailProduct.ID == thumbnailID && thumbnailProduct.Ativo
                          select thumbnailProduct).FirstOrDefaultAsync();
        }

        public async Task UpdateAsync(ThumbnailProduct produtoThumbnail)
        {
            produtoThumbnail.DataEdicao = DateTime.Now;
            await Task.FromResult(0);
        }

        public async Task<ThumbnailProduct> GetByIDAsync(int produtoThumbnailID, int empresaID)
        {
            return await (from produtoThumbnail in context.ProdutoThumbnail
                          where produtoThumbnail.ID == produtoThumbnailID &&
                                produtoThumbnail.Ativo == true
                          select produtoThumbnail
                        ).FirstOrDefaultAsync();
        }
    }
}
