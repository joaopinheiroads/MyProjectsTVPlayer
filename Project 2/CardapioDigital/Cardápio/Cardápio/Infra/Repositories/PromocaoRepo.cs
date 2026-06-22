using Cardápio.Infra.Data;
using Cardápio.Infra.Model;
using Microsoft.EntityFrameworkCore;

namespace Cardápio.Infra.Repositories
{
    public class PromocaoRepo
    {
        private readonly AppDbContext _context;

        public PromocaoRepo(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Promocao>> GetPromocoesByEmpresa(int empresaId)
        {
            return await _context.Promocoes
                .Where(p => p.EmpresaID == empresaId && p.Ativo)
                .Include(p => p.PromocaoHorarios.Where(h => h.Ativo))
                .Include(p => p.ProdutoPromocoes.Where(pp => pp.Ativo))
                    .ThenInclude(pp => pp.Produto)
                .OrderByDescending(p => p.DataCadastro)
                .ToListAsync();
        }

        public async Task<Promocao?> GetById(int id)
        {
            return await _context.Promocoes
                .Include(p => p.PromocaoHorarios.Where(h => h.Ativo))
                .Include(p => p.ProdutoPromocoes.Where(pp => pp.Ativo))
                    .ThenInclude(pp => pp.Produto)
                .Include(p => p.Empresa)
                .FirstOrDefaultAsync(p => p.ID == id);
        }

        public async Task<Promocao> Add(Promocao promocao)
        {
            _context.Promocoes.Add(promocao);
            await _context.SaveChangesAsync();
            return promocao;
        }

        public async Task<Promocao> Update(Promocao promocao)
        {
            _context.Promocoes.Update(promocao);
            await _context.SaveChangesAsync();
            return promocao;
        }

        public async Task<bool> Delete(int id)
        {
            var promocao = await GetById(id);
            if (promocao == null) return false;

            promocao.Ativo = false;
            promocao.DataEdicao = DateTime.Now;
            
            _context.Promocoes.Update(promocao);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<Promocao>> GetPromocoesAtivas(int empresaId, DateTime dataAtual)
        {
            return await _context.Promocoes
                .Where(p => p.EmpresaID == empresaId && 
                           p.Ativo &&
                           p.DataInicio <= dataAtual &&
                           p.DataFim >= dataAtual)
                .Include(p => p.PromocaoHorarios.Where(h => h.Ativo))
                .Include(p => p.ProdutoPromocoes.Where(pp => pp.Ativo))
                    .ThenInclude(pp => pp.Produto)
                .ToListAsync();
        }

        public async Task<List<Promocao>> GetPromocoesByProduto(int produtoId)
        {
            return await _context.Promocoes
                .Where(p => p.Ativo && p.ProdutoPromocoes.Any(pp => pp.ProdutoID == produtoId && pp.Ativo))
                .Include(p => p.PromocaoHorarios.Where(h => h.Ativo))
                .Include(p => p.ProdutoPromocoes.Where(pp => pp.ProdutoID == produtoId && pp.Ativo))
                .ToListAsync();
        }
    }
}
