using Cardápio.Infra.Data;
using Cardápio.Infra.Interfaces.Repositories;
using Cardápio.Infra.Model;
using Microsoft.EntityFrameworkCore;

namespace Cardápio.Infra.Repositories
{
    public class CodigoVerificacaoRepo : ICodigoVerificacoRepo
    {
        private readonly AppDbContext context;

        public CodigoVerificacaoRepo(AppDbContext context)
        {
            this.context = context;
        }

        public async Task AddAsync(CodigoVerificacao entity)
        {
            try
            {
                if (entity == null)
                {
                    throw new ArgumentNullException(nameof(entity));
                }
                await context.CodigoVerificacao.AddAsync(entity);
            }
            catch
            {
                throw;
            }
        }

        public async Task<CodigoVerificacao> GetByEmail(string email)
        {
            return await (from codigoVerificacao in context.CodigoVerificacao
                          where codigoVerificacao.Email == email &&
                                codigoVerificacao.Ativo == true
                          select codigoVerificacao
            ).FirstOrDefaultAsync();
        }

        public async Task<CodigoVerificacao> GetByPhone(string phone)
        {
            return await (from codigoVerificacao in context.CodigoVerificacao
                          where codigoVerificacao.Celular == phone &&
                                codigoVerificacao.Ativo == true
                          select codigoVerificacao
            ).FirstOrDefaultAsync();
        }

        public async Task<CodigoVerificacao> GetByIDAsync(int codeID, int empresaID)
        {
            return await (from codigoVerificacao in context.CodigoVerificacao
                          where codigoVerificacao.ID == codeID &&
                                codigoVerificacao.Ativo == true
                          select codigoVerificacao
                        ).FirstOrDefaultAsync();
        }

        public async Task<CodigoVerificacao> GetByCodeAndEmail(string email, string code)
        {
            return await (from codigoVerificacao in context.CodigoVerificacao
                          where codigoVerificacao.Codigo == code &&
                                codigoVerificacao.Email == email &&
                                codigoVerificacao.Ativo == true
                          select codigoVerificacao
            ).FirstOrDefaultAsync();
        }

        public async Task<CodigoVerificacao> GetByCodeAndPhone(string phone, string code)
        {
            return await (from codigoVerificacao in context.CodigoVerificacao
                          where codigoVerificacao.Codigo == code &&
                                codigoVerificacao.Celular == phone &&
                                codigoVerificacao.Ativo == true
                          select codigoVerificacao
                        ).FirstOrDefaultAsync();
        }

        public async Task UpdateAsync(CodigoVerificacao entity)
        {
            await Task.FromResult(0);
        }
    }
}
