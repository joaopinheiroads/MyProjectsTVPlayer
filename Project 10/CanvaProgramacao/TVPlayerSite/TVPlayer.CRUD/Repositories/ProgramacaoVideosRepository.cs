using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TVPlayer.CRUD.Interfaces.Repositories;
using TVPlayer.CRUD.Models;

namespace TVPlayer.CRUD.Repositories
{
    public class ProgramacaoVideosRepository : IProgramacaoVideosRepository
    {
        private VideoContext _context;

        public ProgramacaoVideosRepository(VideoContext context) => _context = context;

        public async Task<IEnumerable<ProgramacaoVideos>> GetProgramacaoVideosByProgramacaoIDAsync(int programacaoID)
        {
            return await _context.ProgramacaoVideos.Where(pv => pv.ProgramacaoId == programacaoID && pv.Ativo == true).ToListAsync();
        }

        public async Task<IEnumerable<ProgramacaoVideos>> GetProgramacaoVideosOrdenadosByProgramacaoIDAsync(int programacaoID)
        {
            return await _context.ProgramacaoVideos
                .Include(pv => pv.Video)
                .Include(pv => pv.Rssusuario)
                .Include(pv => pv.Campanha)
                .Where(pv => pv.ProgramacaoId == programacaoID && pv.Ativo == true)
                .OrderBy(pv => pv.Ordem)
                .ThenBy(pv => pv.Id)
                .ToListAsync();
        }

        public async Task<int> GetProximaOrdemAsync(int programacaoID)
        {
            var maiorOrdem = await _context.ProgramacaoVideos
                .Where(pv => pv.ProgramacaoId == programacaoID && pv.Ativo == true)
                .MaxAsync(pv => pv.Ordem);

            return maiorOrdem.GetValueOrDefault() + 1;
        }

        public async Task AddAsync(ProgramacaoVideos entity)
        {
            try { await _context.ProgramacaoVideos.AddAsync(entity); }
            catch (Exception ex) { throw ex; }
        }
    }
}