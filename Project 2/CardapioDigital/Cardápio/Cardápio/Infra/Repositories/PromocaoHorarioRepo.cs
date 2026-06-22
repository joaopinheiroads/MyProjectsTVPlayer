using Cardápio.Infra.Data;
using Cardápio.Infra.Model;
using Microsoft.EntityFrameworkCore;

namespace Cardápio.Infra.Repositories
{
    public class PromocaoHorarioRepo
    {
        private readonly AppDbContext _context;

        public PromocaoHorarioRepo(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<PromocaoHorario>> GetHorariosByPromocaoId(int promocaoId)
        {
            return await _context.PromocaoHorarios
                .Where(h => h.PromocaoID == promocaoId && h.Ativo)
                .Include(h => h.Promocao)
                .OrderBy(h => h.DiaSemana)
                .ThenBy(h => h.HoraInicio)
                .ToListAsync();
        }

        public async Task<List<PromocaoHorario>> GetHorariosByEmpresa(int empresaId)
        {
            return await _context.PromocaoHorarios
                .Include(h => h.Promocao)
                .Where(h => h.Promocao!.EmpresaID == empresaId && h.Ativo)
                .OrderBy(h => h.Promocao!.Nome)
                .ThenBy(h => h.DiaSemana)
                .ThenBy(h => h.HoraInicio)
                .ToListAsync();
        }

        public async Task<PromocaoHorario?> GetById(int id)
        {
            return await _context.PromocaoHorarios
                .Include(h => h.Promocao)
                .FirstOrDefaultAsync(h => h.ID == id);
        }

        public async Task<PromocaoHorario> Add(PromocaoHorario promocaoHorario)
        {
            _context.PromocaoHorarios.Add(promocaoHorario);
            await _context.SaveChangesAsync();
            return promocaoHorario;
        }

        public async Task<PromocaoHorario> Update(PromocaoHorario promocaoHorario)
        {
            _context.PromocaoHorarios.Update(promocaoHorario);
            await _context.SaveChangesAsync();
            return promocaoHorario;
        }

        public async Task<bool> Delete(int id)
        {
            var horario = await GetById(id);
            if (horario == null) return false;

            horario.Ativo = false;
            horario.DataEdicao = DateTime.Now;
            
            _context.PromocaoHorarios.Update(horario);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<PromocaoHorario>> GetHorariosAtivos(DateTime dataAtual, string diaSemana, string horaAtual)
        {
            return await _context.PromocaoHorarios
                .Include(h => h.Promocao)
                .Where(h => h.Ativo && 
                           h.Promocao!.Ativo &&
                           h.Promocao.DataInicio <= dataAtual &&
                           h.Promocao.DataFim >= dataAtual &&
                           h.DiaSemana == diaSemana &&
                           string.Compare(h.HoraInicio, horaAtual) <= 0 &&
                           string.Compare(h.HoraFim, horaAtual) >= 0)
                .ToListAsync();
        }
    }
}
