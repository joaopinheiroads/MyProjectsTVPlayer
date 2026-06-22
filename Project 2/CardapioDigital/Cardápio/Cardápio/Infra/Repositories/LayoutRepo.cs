using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using Cardápio.Dto;
using Cardápio.Infra.Model;
using Cardápio.Infra.Interfaces.Repositories;
using Cardápio.Infra.Data;

namespace Cardápio.Infra.Repositories
{
    public class LayoutRepo : ILayoutRepo
    {
        private readonly AppDbContext context;

        public LayoutRepo(AppDbContext context)
        {
            this.context = context;
        }
        public async Task AddAsync(Layout layout)
        {
            try
            {
                if (layout == null)
                {
                    throw new ArgumentNullException(nameof(layout));
                }
                await context.Layout.AddAsync(layout);
            }
            catch
            {
                throw;
            }
        }

        public async Task<Layout> GetByIDAsync(int layoutID, int empresaID)
        {
            return null;
        }

        public async Task<LayoutGetDTO> GetLayoutByEmpresaIDAsync(int empresaID)
        {
            return null;
        }

        public async Task<LayoutGetDTO> GetLayoutByIDAsync(int layoutID, int empresaID)
        {
            return null;
        }

        private static LogoGetDTO ConvertLogo(Logo logo)
        {
            if (logo == null)
                return null;

            LogoGetDTO logoGetDTO = new LogoGetDTO();
            logoGetDTO.ID = logo.ID;
            logoGetDTO.Nome = logo.Nome;
            logoGetDTO.Arquivo = logo.Arquivo;

            return logoGetDTO;
        }

        private static Collection<BannerGetDTO> ConvertBanner(ICollection<Banner> colBanner)
        {
            Collection<BannerGetDTO> colBannerGetDTO = new();

            foreach (Banner banner in colBanner)
            {
                BannerGetDTO bannerGetDTO = new BannerGetDTO();
                bannerGetDTO.ID = banner.ID;
                bannerGetDTO.Nome = banner.Nome;
                bannerGetDTO.Arquivo = banner.Arquivo;
                colBannerGetDTO.Add(bannerGetDTO);
            }
            return colBannerGetDTO;
        }

        public async Task UpdateAsync(Layout layout)
        {
            layout.DataEdicao = DateTime.Now;

            await Task.FromResult(0);
        }
    }
}
