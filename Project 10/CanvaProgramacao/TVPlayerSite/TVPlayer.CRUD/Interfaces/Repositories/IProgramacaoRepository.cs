using System.Collections.Generic;
using System.Threading.Tasks;
using TVPlayer.CRUD.Classes;
using TVPlayer.CRUD.Models;

namespace TVPlayer.CRUD.Interfaces.Repositories
{
    public interface IProgramacaoRepository
    {
        Task<IEnumerable<ProgramacaoResumo>> GetProgramacoesByUsuarioIDAsync(int usuarioID);
        Task AddAsync(Programacao entity);
    }
}