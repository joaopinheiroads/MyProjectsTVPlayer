using System.Threading.Tasks;

namespace TVPlayerSite.API.Interfaces.CRM
{
    public interface IDemonstracaoService
    {

        Task<bool> EnviarCodigoPorEmail(string email, string nome, string codigo);
    }
}
