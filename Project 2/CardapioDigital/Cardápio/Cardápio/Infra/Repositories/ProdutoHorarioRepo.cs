using Cardápio.Infra.Data;
using Cardápio.Infra.Interfaces.Repositories;
using Cardápio.Infra.Model;
using Microsoft.EntityFrameworkCore;

namespace Cardápio.Infra.Repositories
{
    public class ProdutoHorarioRepo : IProdutoHorarioRepo
    {
        private readonly AppDbContext _context;

        public ProdutoHorarioRepo(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<ProdutoHorario>> GetHorariosByProdutoId(int produtoId)
        {
            return await _context.ProdutoHorarios
                .Where(h => h.ProdutoID == produtoId && h.Ativo)
                .OrderBy(h => h.HoraInicio)
                .ToListAsync();
        }

        public async Task<List<ProdutoHorario>> GetHorariosByEmpresa(int empresaId)
        {
            return await _context.ProdutoHorarios
                .Include(h => h.Produto)
                .Where(h => h.Produto.EmpresaID == empresaId && h.Ativo)
                .OrderBy(h => h.ProdutoID)
                .ThenBy(h => h.HoraInicio)
                .ToListAsync();
        }

        public async Task<ProdutoHorario?> GetById(int id)
        {
            return await _context.ProdutoHorarios
                .Include(h => h.Produto)
                .FirstOrDefaultAsync(h => h.ID == id);
        }

        public async Task<ProdutoHorario> Add(ProdutoHorario produtoHorario)
        {
            _context.ProdutoHorarios.Add(produtoHorario);
            await _context.SaveChangesAsync();
            return produtoHorario;
        }

        public async Task<ProdutoHorario> Update(ProdutoHorario produtoHorario)
        {
            _context.ProdutoHorarios.Update(produtoHorario);
            await _context.SaveChangesAsync();
            return produtoHorario;
        }

        public async Task<bool> Delete(int id)
        {
            var horario = await GetById(id);
            if (horario == null) return false;

            horario.Ativo = false;
            horario.DataEdicao = DateTime.Now;
            await Update(horario);
            return true;
        }

        public async Task<bool> IsProdutoDisponivelNoHorario(int produtoId, DateTime agora)
        {
            // Logs removidos para limpeza
            
            // Buscar horários configurados para este produto
            var horariosConfigurados = await _context.ProdutoHorarios
                .Where(h => h.ProdutoID == produtoId && h.Ativo)
                .ToListAsync();
            
            // Se não há horários configurados, produto está disponível 24/7
            if (!horariosConfigurados.Any())
            {
                return true;
            }
            
            // Verificar se há horário para o dia atual
            string diaAtual = GetDiaSemanaPortugues(agora.DayOfWeek);
            var horarioHoje = horariosConfigurados.FirstOrDefault(h => h.DiaSemana == diaAtual);
            
            if (horarioHoje == null)
            {
                return false;
            }
            
            // Verificar se a hora atual está no intervalo
            var horaAtual = agora.TimeOfDay;
            var horaInicio = TimeSpan.Parse(horarioHoje.HoraInicio);
            var horaFim = TimeSpan.Parse(horarioHoje.HoraFim);
            
            bool disponivel = horaAtual >= horaInicio && horaAtual <= horaFim;
            
            return disponivel;
        }

        private static string GetDiaSemanaPortugues(DayOfWeek diaSemana)
        {
            return diaSemana switch
            {
                DayOfWeek.Sunday => "Domingo",
                DayOfWeek.Monday => "Segunda",
                DayOfWeek.Tuesday => "Terça",
                DayOfWeek.Wednesday => "Quarta",
                DayOfWeek.Thursday => "Quinta",
                DayOfWeek.Friday => "Sexta",
                DayOfWeek.Saturday => "Sábado",
                _ => "Domingo"
            };
        }
    }
}
