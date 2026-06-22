using Cardápio.Infra.Model;

namespace Cardápio.Infra.Interfaces.Repositories
{
    public interface ICodigoVerificacoRepo : IRepo<CodigoVerificacao>
    {
        public Task<CodigoVerificacao> GetByEmail(string email);

        public Task<CodigoVerificacao> GetByPhone(string phone);

        public Task<CodigoVerificacao> GetByCodeAndEmail(string email, string code);
        public Task<CodigoVerificacao> GetByCodeAndPhone(string phone, string code);

    }
}
