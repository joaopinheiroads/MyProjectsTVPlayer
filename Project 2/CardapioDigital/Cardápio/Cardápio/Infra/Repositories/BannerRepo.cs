using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System;
using Cardápio.Client.Pages;
using Cardápio.Infra.Model;
using Cardápio.Infra.Interfaces.Repositories;
using Cardápio.Infra.Data;

namespace Cardápio.Infra.Repositories
{
    public class BannerRepo : IBannerRepo
    {
        private readonly AppDbContext context;

        public BannerRepo(AppDbContext context)
        {
            this.context = context;
        }
        public async Task AddAsync(Banner banner)
        {
            try
            {
                if (banner == null)
                {
                    throw new ArgumentNullException(nameof(banner));
                }
                await context.Banner.AddAsync(banner);
            }
            catch
            {
                throw;
            }
        }

        public async Task<Banner> GetByIDAsync(int bannerID, int empresaID)
        {
            return null;
        }

        public async Task UpdateAsync(Banner banner)
        {
            banner.DataEdicao = DateTime.Now;

            await Task.FromResult(0);
        }

        public async Task<Banner> GetByCompanyIDAsync(int empresaID)
        {
            return await context.Banner.Where(banner => banner.EmpresaID == empresaID).FirstOrDefaultAsync();
        }
    }
}
