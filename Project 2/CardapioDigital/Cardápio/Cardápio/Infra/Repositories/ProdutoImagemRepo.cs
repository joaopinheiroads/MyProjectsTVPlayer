
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System;
using Cardápio.Dto;
using Cardápio.Infra.Model;
using Cardápio.Infra.Interfaces.Repositories;
using Cardápio.Infra.Data;

namespace Cardápio.Infra.Repositories
{
    public class ProdutoImagemRepo : IProdutoImagemRepo
    {
        private readonly AppDbContext context;

        public ProdutoImagemRepo(AppDbContext context)
        {
            this.context = context;
        }
        public async Task AddAsync(ImageProduct produtoImagem)
        {
            try
            {
                if (produtoImagem == null)
                {
                    throw new ArgumentNullException(nameof(produtoImagem));
                }
                await context.ProdutoImagem.AddAsync(produtoImagem);
            }
            catch
            {
                throw;
            }
        }

        public async Task<ImageProduct> GetByIDAsync(int produtoImagemID, int empresaID)
        {
            return await (from produtoImagem in context.ProdutoImagem
                          join produto in context.Produto on produtoImagem.ProdutoID equals produto.ID
                          where produtoImagem.ID == produtoImagemID &&
                                produtoImagem.Ativo == true &&
                                produto.EmpresaID == empresaID
                          select produtoImagem
              ).FirstOrDefaultAsync();
        }

        public async Task<ImageProduct> GetProdutoImagemByProdutoIDAsync(int produtoID)
        {
            return await (from produtoImagem in context.ProdutoImagem
                          where produtoImagem.Ativo == true &&
                                produtoImagem.ProdutoID == produtoID
                          select new ImageProduct
                          {
                              ID = produtoImagem.ID,
                              Nome = produtoImagem.Nome,
                              Arquivo = produtoImagem.Arquivo,
                              Altura = produtoImagem.Altura,
                              Ativo = produtoImagem.Ativo,
                              DataCadastro = produtoImagem.DataCadastro,
                              DataEdicao = produtoImagem.DataEdicao,
                              Produto = produtoImagem.Produto,
                              ProdutoID = produtoImagem.ID,
                              Largura = produtoImagem.Largura,
                              Tamanho = produtoImagem.Tamanho,
                              UsuarioCadastro = produtoImagem.UsuarioCadastro,
                              UsuarioEdicao = produtoImagem.UsuarioEdicao,
                              UsuarioIDCadastro = produtoImagem.UsuarioIDCadastro,
                              UsuarioIDEdicao = produtoImagem.UsuarioIDEdicao,
                          }).FirstOrDefaultAsync();
        }

        public async Task UpdateAsync(ImageProduct produtoImagem)
        {
            produtoImagem.DataEdicao = DateTime.Now;

            await Task.FromResult(0);
        }
    }
}
