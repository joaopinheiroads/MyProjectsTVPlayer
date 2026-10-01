using System.Threading.Tasks;

namespace TVPlayerAPI.UOL.Fontes
{
    public interface IFonteHtml
    {
        string Nome { get; }

        Task<string> ObterHtmlAsync(string url);
    }
}
