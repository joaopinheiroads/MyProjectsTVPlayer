using Cardápio.Dto;
using Cardápio.Infra.Data;
using Cardápio.Infra.Interfaces.Repositories;
using Cardápio.Infra.Model;
using Microsoft.EntityFrameworkCore;

namespace Cardápio.Infra.Repositories
{
    public class QrCodeLayoutRepo : IQrCodeLayoutRepo
    {
        private readonly AppDbContext context;

        public QrCodeLayoutRepo(AppDbContext context)
        {
            this.context = context;
        }

        public async Task AddAsync(QrCodeLayout qrCodeLayout)
        {
            try
            {
                if (qrCodeLayout == null)
                {
                    throw new ArgumentNullException(nameof(qrCodeLayout));
                }
                await context.QrCodeLayout.AddAsync(qrCodeLayout);
            }
            catch
            {
                throw;
            }
        }

        public async Task<QrCodeLayout> GetByIDAsync(int qrCodeLayoutID, int empresaID)
        {
            return await (from qrCodeLayout in context.QrCodeLayout
                          join layout in context.Layout on qrCodeLayout.ID equals layout.LogoID
                          where qrCodeLayout.ID == qrCodeLayoutID &&
                                qrCodeLayout.Ativo == true &&
                                layout.EmpresaID == empresaID
                          select qrCodeLayout
              ).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<QrCodeLayoutGetDTO>> GetByEmpresaIDAsync(int empresaID)
        {
            return await (from qrCodeLayout in context.QrCodeLayout
                          where qrCodeLayout.EmpresaID == empresaID
                          select new QrCodeLayoutGetDTO()
                          {
                              ID = qrCodeLayout.ID,
                              Nome = qrCodeLayout.Nome,
                              TitleText = qrCodeLayout.TextoTitulo,
                              DescriptionText = qrCodeLayout.TextoDescricao,
                              BackgroundColor = qrCodeLayout.CorFundo,
                              BorderColor = qrCodeLayout.CorBorda,
                              BorderRadius = qrCodeLayout.RadioBorda,
                              TextColor = qrCodeLayout.CorTexto,
                          }).ToListAsync();
        }

        public async Task UpdateAsync(QrCodeLayout qrCodeLayout)
        {
            qrCodeLayout.DataEdicao = DateTime.Now;

            await Task.FromResult(0);
        }
    }
}
