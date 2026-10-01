using System.Collections.Generic;
using System.Threading.Tasks;
using TVPlayerAPI.G1.Classes;

namespace TVPlayerAPI.G1.Interfaces
{
    internal interface IFonteNoticiaG1
    {
        string Nome { get; }

        Task<IReadOnlyList<NoticiaG1>> ObterNoticiasAsync(string caminhoDaSecao, int quantidade);
    }
}
