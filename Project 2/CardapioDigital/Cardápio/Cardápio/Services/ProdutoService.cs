
using System.Threading.Tasks;
using Cardápio.Dto;
using Microsoft.EntityFrameworkCore;
using Cardápio.Infra.Model;

namespace Cardápio.Services
{
    public class ProdutoService
    {
        private readonly DbContext _context;

        public ProdutoService(DbContext context)
        {
            _context = context;
        }

        public async Task<List<Product>> GetAllAsync()
        {
            await RemoverPromocoesExpiradasAsync();
            return await _context.Set<Product>().ToListAsync();
        }

        /// <summary>
        /// Remove promoções expiradas dos produtos.
        /// </summary>
        public async Task RemoverPromocoesExpiradasAsync()
        {
            var agora = DateTime.Now;
            var produtosExpirados = await _context.Set<Product>()
                .Where(p => p.Promocao && p.FimPromocao != null && p.FimPromocao <= agora)
                .ToListAsync();

            foreach (var produto in produtosExpirados)
            {
                produto.Promocao = false;
                produto.PrecoPromocional = null;
                produto.FimPromocao = null;
            }

            if (produtosExpirados.Count > 0)
            {
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> UpdateAsync(int id, ProdutoUpdateDTO dto)
        {
            // Antes de atualizar, remove promoções expiradas
            await RemoverPromocoesExpiradasAsync();
            var produto = await _context.Set<Product>()
                .Include(p => p.ColProdutoImagem)
                .FirstOrDefaultAsync(p => p.ID == id);

            if (produto == null)
                return false;

            produto.Nome = dto.Nome;
            produto.Descricao = dto.Descricao;
            produto.Preco = dto.Preco;
            produto.CategoriaID = dto.CategoriaID;
            produto.Promocao = dto.Promocao;
            produto.PrecoPromocional = dto.Promocao ? dto.PrecoPromocional : null;
            produto.FimPromocao = dto.Promocao ? dto.FimPromocao : null;
            produto.Destaque = dto.Destaque;
            produto.Ativo = dto.Ativo;
           produto.QTDPessoa = dto.QTDPessoa;

            // Certifique-se de que ao atualizar ou criar produtos, os campos promocionais s�o tratados corretamente.
            // ...atualiza��o de imagens e outros campos...

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> AddAsync(ProdutoAddDTO dto)
        {
            var produto = new Product
            {
                Nome = dto.Nome,
                Descricao = dto.Descricao,
                Preco = dto.Preco,
                CategoriaID = dto.CategoriaID,
                QTDPessoa = dto.QTDPessoa,
                Promocao = dto.Promocao,
                PrecoPromocional = dto.Promocao ? dto.PrecoPromocional : null,
                FimPromocao = dto.Promocao ? dto.FimPromocao : null,
                Destaque = dto.Destaque,
                Ativo = dto.Ativo,
                // Adicione outros campos necess�rios conforme seu modelo
            };

            _context.Set<Product>().Add(produto);
            await _context.SaveChangesAsync();
            return produto.ID;
        }
    }
}