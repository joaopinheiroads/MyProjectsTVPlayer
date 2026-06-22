using Cardápio.Infra.Data;
using Cardápio.Infra.Model;
using Cardápio.Infra.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Cardápio.Infra.Repositories
{
    /// <summary>
    /// Repositório para gerenciar promoções por produto, dia da semana e horário
    /// </summary>
    public class ProdutoPromocaoHorarioRepo : IProdutoPromocaoHorarioRepo
    {
        private readonly AppDbContext _context;

        public ProdutoPromocaoHorarioRepo(AppDbContext context)
        {
            _context = context;
        }

        #region Métodos IRepo<ProdutoPromocaoHorario>

        public async Task AddAsync(ProdutoPromocaoHorario entity)
        {
            await _context.ProdutoPromocaoHorarios.AddAsync(entity);
            // Não fazer SaveChanges aqui - Unit of Work gerencia transações
        }

        public async Task<ProdutoPromocaoHorario> GetByIDAsync(int ID, int empresaID)
        {
            return await _context.ProdutoPromocaoHorarios
                .Where(p => p.ID == ID)
                .Where(p => _context.Produto.Any(prod => 
                    prod.ID == p.ProdutoID && 
                    prod.EmpresaID == empresaID))
                .FirstOrDefaultAsync();
        }

        public async Task UpdateAsync(ProdutoPromocaoHorario entity)
        {
            _context.ProdutoPromocaoHorarios.Update(entity);
            // Não fazer SaveChanges aqui - Unit of Work gerencia transações
        }

        #endregion

        #region Métodos IProdutoPromocaoHorarioRepo

        public async Task<List<ProdutoPromocaoHorario>> GetPromocoesByProdutoIdAsync(int produtoId)
        {
            return await _context.ProdutoPromocaoHorarios
                .Where(p => p.ProdutoID == produtoId && p.Ativo)
                .OrderBy(p => p.DiaSemana)
                .ThenBy(p => p.HoraInicioString)
                .ToListAsync();
        }

        /*public async Task<List<ProdutoPromocaoHorario>> GetAllPromocoesByProdutoIdAsync(int produtoId)
        {
            return await _context.ProdutoPromocaoHorarios
                .Where(p => p.ProdutoID == produtoId)
                .OrderBy(p => p.DiaSemana)
                .ThenBy(p => p.HoraInicioString)
                .ToListAsync();
        }*/

        public async Task<ProdutoPromocaoHorario?> GetPromocaoAtivaAsync(int produtoId, string diaSemana, TimeSpan horaAtual)
        {
            var horaAtualString = horaAtual.ToString(@"hh\:mm");
            
            // Buscar todas as promoções do produto no dia e filtrar em memória
            var promocoesDisponiveis = await _context.ProdutoPromocaoHorarios
                .Where(p => p.ProdutoID == produtoId && 
                           p.DiaSemana == diaSemana && 
                           p.Ativo)
                .ToListAsync();
                
            // Filtrar em memória usando as propriedades TimeSpan convertidas
            return promocoesDisponiveis
                .Where(p => p.HoraInicio <= horaAtual && p.HoraFim >= horaAtual)
                .OrderBy(p => p.PrecoPromocional)
                .FirstOrDefault();
        }

        public async Task<List<ProdutoPromocaoHorario>> GetPromocoesAtivasAsync()
        {
            return await _context.ProdutoPromocaoHorarios
                .Where(p => p.Ativo)
                .OrderBy(p => p.ProdutoID)
                .ThenBy(p => p.DiaSemana)
                .ThenBy(p => p.HoraInicioString)
                .ToListAsync();
        }

        public async Task<List<ProdutoPromocaoHorario>> GetPromocoesByDiaSemanaAsync(string diaSemana)
        {
            return await _context.ProdutoPromocaoHorarios
                .Where(p => p.DiaSemana == diaSemana && p.Ativo)
                .OrderBy(p => p.ProdutoID)
                .ThenBy(p => p.HoraInicioString)
                .ToListAsync();
        }

        public async Task<bool> VerificarConflitosHorarioAsync(int produtoId, string diaSemana, string horaInicio, string horaFim, int? promocaoId = null)
        {
            var conflitos = await VerificarConflitosHorario(produtoId, diaSemana, horaInicio, horaFim, promocaoId);
            return conflitos.Any();
        }

        public async Task DesativarPromocoesProdutoAsync(int produtoId)
        {
            var promocoes = await _context.ProdutoPromocaoHorarios
                .Where(p => p.ProdutoID == produtoId && p.Ativo)
                .ToListAsync();

            foreach (var promocao in promocoes)
            {
                promocao.Ativo = false;
                promocao.DataEdicao = DateTime.Now;
            }

            // Não fazer SaveChanges aqui - Unit of Work gerencia transações
        }

        /// <summary>
        /// Desativa uma promoção específica por ID
        /// </summary>
        public async Task<bool> DesativarPromocaoAsync(int promocaoId, int usuarioId)
        {
            var promocao = await GetById(promocaoId);
            if (promocao == null || !promocao.Ativo) return false;

            promocao.Ativo = false;
            promocao.DataEdicao = DateTime.Now;
            promocao.UsuarioIDEdicao = usuarioId;

            
            // Não fazer SaveChanges aqui - Unit of Work gerencia transações
            return true;
        }

        #endregion

        #region Métodos Adicionais

        /// <summary>
        /// Busca promoção por ID (método adicional)
        /// </summary>
        public async Task<ProdutoPromocaoHorario?> GetById(int id)
        {
            return await _context.ProdutoPromocaoHorarios
                .FirstOrDefaultAsync(p => p.ID == id);
        }

        /// <summary>
        /// Busca produto por ID
        /// </summary>
        public async Task<Product?> GetProdutoById(int produtoId)
        {
            return await _context.Produto
                .FirstOrDefaultAsync(p => p.ID == produtoId && p.Ativo && !p.Excluido);
        }

        /// <summary>
        /// Obtém todas as promoções de um produto (método adicional)
        /// </summary>
        public async Task<List<ProdutoPromocaoHorario>> GetPromocoesByProdutoId(int produtoId)
        {
            return await _context.ProdutoPromocaoHorarios
                .Where(p => p.ProdutoID == produtoId && p.Ativo)
                .OrderBy(p => p.DiaSemana)
                .ThenBy(p => p.HoraInicioString)
                .ToListAsync();
        }

        /// <summary>
        /// Verifica se produto tem promoção ativa no horário especificado
        /// </summary>
        public async Task<decimal?> GetPrecoPromocionalAtivo(int produtoId, string diaSemana, string horaAtual)
        {
            try
            {
                // Converter horário atual para TimeSpan para comparação
                var horaAtualTimeSpan = TimeSpan.Parse(horaAtual);
                
                // Buscar todas as promoções do produto no dia e fazer comparação em memória
                var promocoesDisponiveis = await _context.ProdutoPromocaoHorarios
                    .Where(p => p.ProdutoID == produtoId && 
                               p.DiaSemana == diaSemana && 
                               p.Ativo)
                    .ToListAsync();
                
                // Filtrar em memória usando as propriedades TimeSpan convertidas
                var promocao = promocoesDisponiveis
                    .Where(p => p.HoraInicio <= horaAtualTimeSpan && p.HoraFim >= horaAtualTimeSpan)
                    .OrderBy(p => p.PrecoPromocional)
                    .FirstOrDefault();

                if (promocao != null)
                {
                    return promocao.PrecoPromocional;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        /// <summary>
        /// Lista produtos que têm promoção ativa no horário atual
        /// </summary>
        public async Task<List<Product>> GetProdutosComPromocaoAtiva(string diaSemana, string horaAtual, int? empresaId = null)
        {
            var horaAtualTimeSpan = TimeSpan.Parse(horaAtual);
            
            // Primeiro buscar todas as promoções ativas do dia
            var promocoesAtivasQuery = _context.ProdutoPromocaoHorarios
                .Where(pph => pph.DiaSemana == diaSemana && pph.Ativo);
                
            if (empresaId.HasValue)
            {
                promocoesAtivasQuery = promocoesAtivasQuery
                    .Where(pph => _context.Produto.Any(p => p.ID == pph.ProdutoID && p.EmpresaID == empresaId.Value));
            }
            
            var promocoesAtivas = await promocoesAtivasQuery.ToListAsync();
            
            // Filtrar em memória para horários que estão ativos agora
            var produtosComPromocao = promocoesAtivas
                .Where(pph => pph.HoraInicio <= horaAtualTimeSpan && pph.HoraFim >= horaAtualTimeSpan)
                .Select(pph => pph.ProdutoID)
                .Distinct()
                .ToList();
            
            // Buscar os produtos correspondentes
            var query = _context.Produto
                .Where(p => p.Ativo && !p.Excluido && produtosComPromocao.Contains(p.ID));

            return await query.ToListAsync();
        }

        /// <summary>
        /// Obtém estatísticas de promoções por empresa
        /// </summary>
        public async Task<Dictionary<string, object>> GetEstatisticasPromocoes(int empresaId)
        {
            var totalProdutos = await _context.Produto
                .CountAsync(p => p.EmpresaID == empresaId && p.Ativo && !p.Excluido);

            var produtosComPromocao = await _context.Produto
                .Where(p => p.EmpresaID == empresaId && p.Ativo && !p.Excluido)
                .Where(p => _context.ProdutoPromocaoHorarios.Any(pph => 
                    pph.ProdutoID == p.ID && pph.Ativo))
                .CountAsync();

            var totalPromocoes = await _context.ProdutoPromocaoHorarios
                .Where(pph => _context.Produto.Any(p => 
                    p.ID == pph.ProdutoID && 
                    p.EmpresaID == empresaId && 
                    p.Ativo && 
                    !p.Excluido))
                .Where(pph => pph.Ativo)
                .CountAsync();

            var agora = DateTime.Now;
            var diaSemana = GetDiaSemanaPortugues(agora.DayOfWeek);
            var horaAtual = agora.ToString("HH:mm");
            var horaAtualTimeSpan = TimeSpan.Parse(horaAtual);

            // Buscar promoções ativas do dia atual e filtrar em memória
            var promocoesHoje = await _context.ProdutoPromocaoHorarios
                .Where(pph => _context.Produto.Any(p => 
                    p.ID == pph.ProdutoID && 
                    p.EmpresaID == empresaId && 
                    p.Ativo && 
                    !p.Excluido))
                .Where(pph => pph.DiaSemana == diaSemana && pph.Ativo)
                .ToListAsync();
                
            var promocoesAtivasAgora = promocoesHoje
                .Where(pph => pph.HoraInicio <= horaAtualTimeSpan && pph.HoraFim >= horaAtualTimeSpan)
                .Count();

            return new Dictionary<string, object>
            {
                ["totalProdutos"] = totalProdutos,
                ["produtosComPromocao"] = produtosComPromocao,
                ["percentualProdutosComPromocao"] = totalProdutos > 0 ? Math.Round((double)produtosComPromocao / totalProdutos * 100, 2) : 0,
                ["totalPromocoes"] = totalPromocoes,
                ["promocoesAtivasAgora"] = promocoesAtivasAgora,
                ["diaAtual"] = diaSemana,
                ["horaAtual"] = horaAtual
            };
        }

        /// <summary>
        /// Lista produtos sem promoção configurada
        /// </summary>
        public async Task<List<Product>> GetProdutosSemPromocao(int empresaId)
        {
            return await _context.Produto
                .Where(p => p.EmpresaID == empresaId && p.Ativo && !p.Excluido)
                .Where(p => !_context.ProdutoPromocaoHorarios.Any(pph => 
                    pph.ProdutoID == p.ID && pph.Ativo))
                .OrderBy(p => p.Nome)
                .ToListAsync();
        }

        /// <summary>
        /// Busca promoções por período
        /// </summary>
        public async Task<List<ProdutoPromocaoHorario>> GetPromocoesPorPeriodo(
            int empresaId, 
            DateTime dataInicio, 
            DateTime dataFim)
        {
            return await _context.ProdutoPromocaoHorarios
                .Where(pph => _context.Produto.Any(p => 
                    p.ID == pph.ProdutoID && 
                    p.EmpresaID == empresaId && 
                    p.Ativo && 
                    !p.Excluido))
                .Where(pph => pph.DataCadastro >= dataInicio && pph.DataCadastro <= dataFim)
                .OrderBy(pph => pph.DataCadastro)
                .ToListAsync();
        }

        /// <summary>
        /// Verifica conflitos de horário para um produto em um dia específico
        /// </summary>
        public async Task<List<ProdutoPromocaoHorario>> VerificarConflitosHorario(
            int produtoId, 
            string diaSemana, 
            string horaInicio, 
            string horaFim, 
            int? promocaoIdExcluir = null)
        {
            var query = _context.ProdutoPromocaoHorarios
                .Where(p => p.ProdutoID == produtoId && 
                           p.DiaSemana == diaSemana && 
                           p.Ativo);

            if (promocaoIdExcluir.HasValue)
            {
                query = query.Where(p => p.ID != promocaoIdExcluir.Value);
            }

            var promocoesExistentes = await query.ToListAsync();

            var conflitos = new List<ProdutoPromocaoHorario>();

            foreach (var promocao in promocoesExistentes)
            {
                if (TimeSpan.TryParse(horaInicio, out var novoInicio) &&
                    TimeSpan.TryParse(horaFim, out var novoFim))
                {
                    // Verificar sobreposição de horários (promoção existente já é TimeSpan)
                    if ((novoInicio < promocao.HoraFim && novoFim > promocao.HoraInicio))
                    {
                        conflitos.Add(promocao);
                    }
                }
            }

            return conflitos;
        }

        /// <summary>
        /// Remove permanentemente promoções (use com cuidado)
        /// </summary>
        public async Task<bool> RemoverPromocaoDefinitivamente(int promocaoId, int usuarioId)
        {
            var promocao = await GetById(promocaoId);
            if (promocao == null) return false;

            _context.ProdutoPromocaoHorarios.Remove(promocao);
            // Não fazer SaveChanges aqui - Unit of Work gerencia transações
            return true;
        }

        #endregion

        #region Métodos Auxiliares

        private static string GetDiaSemanaPortugues(DayOfWeek dayOfWeek)
        {
            return dayOfWeek switch
            {
                DayOfWeek.Monday => "Segunda",
                DayOfWeek.Tuesday => "Terça",
                DayOfWeek.Wednesday => "Quarta",
                DayOfWeek.Thursday => "Quinta",
                DayOfWeek.Friday => "Sexta",
                DayOfWeek.Saturday => "Sábado",
                DayOfWeek.Sunday => "Domingo",
                _ => ""
            };
        }

        #endregion
    }
}
