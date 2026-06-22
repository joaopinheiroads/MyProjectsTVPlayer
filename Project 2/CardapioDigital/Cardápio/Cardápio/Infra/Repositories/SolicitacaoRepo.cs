using Cardápio.Client.Pages;
using Cardápio.Infra.Data;
using Cardápio.Infra.Interfaces.Repositories;
using Cardápio.Infra.Model;
using Microsoft.EntityFrameworkCore;

namespace Cardápio.Infra.Repositories
{
    public class SolicitacaoRepo : ISolicitacaoRepo
    {
        private readonly AppDbContext context;

        public SolicitacaoRepo(AppDbContext context)
        {
            this.context = context;
        }

        public async Task AddAsync(SolicitacaoDemonstracao solicitacao)
        {
            try
            {
                if (solicitacao == null)
                {
                    throw new ArgumentNullException(nameof(solicitacao));
                }
                await context.SolicitacaoDemonstracao.AddAsync(solicitacao);
            }
            catch
            {
                throw;
            }
        }

        public async Task<SolicitacaoDemonstracao> GetBySolicitIDAsync(Guid SolicitID)
        {
            return await (from solicitacaoDemonstracao in context.SolicitacaoDemonstracao
                          where solicitacaoDemonstracao.ID == SolicitID
                          select solicitacaoDemonstracao
                        ).FirstOrDefaultAsync();
        }

        public async Task UpdateAsync(SolicitacaoDemonstracao solict)
        {
            await Task.FromResult(0);
        }

        public async Task<SolicitacaoDemonstracao> GetByIDAsync(int solictID, int empresaID)
        {
            return null;
        }
    }
}
