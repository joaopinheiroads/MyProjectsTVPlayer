using System.Threading.Tasks;
using TVPlayerAPI.G1.Classes;

namespace TVPlayerAPI.G1.Interfaces
{
    internal interface IFonteMateriaG1
    {
        Task<MateriaG1> ObterMateriaAsync(string linkDaMateria);
    }
}
