using TVPlayer.CRUD.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TVPlayer.CRUD.Interfaces.Repositories
{
    public interface IProgramacaoVideosRepository
    {
        Task<IEnumerable<ProgramacaoVideos>> GetProgramacaoVideosByProgramacaoIDAsync(int programacaoID);
        Task<IEnumerable<ProgramacaoVideos>> GetProgramacaoVideosOrdenadosByProgramacaoIDAsync(int programacaoID);
        Task<int> GetProximaOrdemAsync(int programacaoID);
        Task AddAsync(ProgramacaoVideos entity);
    }
}