using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System;
using Cardápio.Infra.Model;
using Cardápio.Infra.Interfaces.Repositories;
using Cardápio.Infra.Data;

namespace Cardápio.Infra.Repositories
{
    public class LogoRepo : ILogoRepo
    {
        private readonly AppDbContext context;

        public LogoRepo(AppDbContext context)
        {
            this.context = context;
        }
        public async Task AddAsync(Logo logo)
        {
            try
            {
                if (logo == null)
                {
                    throw new ArgumentNullException(nameof(logo));
                }
                await context.Logo.AddAsync(logo);
            }
            catch
            {
                throw;
            }
        }

        public async Task<Logo> GetByIDAsync(int logoID, int empresaID)
        {
            return await (from logo in context.Logo
                          join layout in context.Layout on logo.ID equals layout.LogoID
                          where logo.ID == logoID &&
                                logo.Ativo == true &&
                                layout.EmpresaID == empresaID
                          select logo
              ).FirstOrDefaultAsync();
        }

        public async Task<Logo> GetByCompanyIDAsync(int empresaID)
        {
            return await context.Logo.Where(logo => logo.EmpresaID == empresaID).FirstOrDefaultAsync();
        }

        public async Task UpdateAsync(Logo logo)
        {
            logo.DataEdicao = DateTime.Now;

            await Task.FromResult(0);
        }
    }
}
