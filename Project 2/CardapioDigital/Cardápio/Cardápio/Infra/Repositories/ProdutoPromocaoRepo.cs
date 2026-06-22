using Cardápio.Infra.Data;
using Cardápio.Infra.Model;
using Microsoft.EntityFrameworkCore;

namespace Cardápio.Infra.Repositories
{
    public class ProdutoPromocaoRepo
    {
        private readonly AppDbContext _context;

        public ProdutoPromocaoRepo(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<ProdutoPromocao>> GetByPromocaoId(int promocaoId)
        {
            return await _context.ProdutoPromocoes
                .Where(pp => pp.PromocaoID == promocaoId && pp.Ativo)
                .Include(pp => pp.Produto)
                .Include(pp => pp.Promocao)
                .ToListAsync();
        }

        public async Task<List<ProdutoPromocao>> GetByProdutoId(int produtoId)
        {
            return await _context.ProdutoPromocoes
                .Where(pp => pp.ProdutoID == produtoId && pp.Ativo)
                .Include(pp => pp.Produto)
                .Include(pp => pp.Promocao)
                .ToListAsync();
        }

        public async Task<ProdutoPromocao?> GetById(int id)
        {
            return await _context.ProdutoPromocoes
                .Include(pp => pp.Produto)
                .Include(pp => pp.Promocao)
                .FirstOrDefaultAsync(pp => pp.ID == id);
        }

        public async Task<ProdutoPromocao?> GetByProdutoAndPromocao(int produtoId, int promocaoId)
        {
            return await _context.ProdutoPromocoes
                .Include(pp => pp.Produto)
                .Include(pp => pp.Promocao)
                .FirstOrDefaultAsync(pp => pp.ProdutoID == produtoId && 
                                         pp.PromocaoID == promocaoId && 
                                         pp.Ativo);
        }

        public async Task<ProdutoPromocao> Add(ProdutoPromocao produtoPromocao)
        {
            _context.ProdutoPromocoes.Add(produtoPromocao);
            await _context.SaveChangesAsync();
            return produtoPromocao;
        }

        public async Task<ProdutoPromocao> Update(ProdutoPromocao produtoPromocao)
        {
            _context.ProdutoPromocoes.Update(produtoPromocao);
            await _context.SaveChangesAsync();
            return produtoPromocao;
        }

        public async Task<bool> Delete(int id)
        {
            var produtoPromocao = await GetById(id);
            if (produtoPromocao == null) return false;

            produtoPromocao.Ativo = false;
            produtoPromocao.DataEdicao = DateTime.Now;
            
            _context.ProdutoPromocoes.Update(produtoPromocao);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<ProdutoPromocao>> GetPromocoesAtivasPorProduto(int produtoId, DateTime dataAtual, string diaSemana, string horaAtual)
        {
            return await _context.ProdutoPromocoes
                .Include(pp => pp.Promocao)
                    .ThenInclude(p => p!.PromocaoHorarios)
                .Include(pp => pp.Produto)
                .Where(pp => pp.ProdutoID == produtoId && 
                           pp.Ativo &&
                           pp.Promocao!.Ativo &&
                           pp.Promocao.DataInicio <= dataAtual &&
                           pp.Promocao.DataFim >= dataAtual &&
                           pp.Promocao.PromocaoHorarios.Any(h => 
                               h.Ativo &&
                               h.DiaSemana == diaSemana &&
                               string.Compare(h.HoraInicio, horaAtual) <= 0 &&
                               string.Compare(h.HoraFim, horaAtual) >= 0))
                .ToListAsync();
        }
    }
}
