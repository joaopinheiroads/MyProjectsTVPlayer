using Cardápio.Client.Pages.Empresas;
using Cardápio.Dto;
using Cardápio.Infra.Data;
using Cardápio.Infra.Interfaces.Repositories;
using Cardápio.Infra.Model;
using Microsoft.EntityFrameworkCore;

namespace Cardápio.Infra.Repositories
{
    public class GrupoAdicionalItemRepo : IGrupoAdicionalItemRepo
    {
        private readonly AppDbContext context;

        public GrupoAdicionalItemRepo(AppDbContext context)
        {
            this.context = context;
        }

        public async Task<IEnumerable<GrupoAdicionalGetDTO>> GetAdditionalGroupByProdutoID(int productID)
        {
            return await (from grupoAdicional in context.GrupoAdicional
                          join grupoTipo in context.TipoGrupoAdicional
                              on grupoAdicional.TipoID equals grupoTipo.ID
                          join produtoGrupo in context.ProdutoGrupoAdicional
                              on grupoAdicional.ID equals produtoGrupo.GrupoAdicionalID
                          where grupoAdicional.Ativo && produtoGrupo.ProdutoID == productID
                          select new GrupoAdicionalGetDTO
                          {
                              ID = grupoAdicional.ID,
                              Nome = grupoAdicional.Nome ?? string.Empty,
                              Ativo = grupoAdicional.Ativo,
                              EmpresaID = grupoAdicional.EmpresaID,
                              Maximo = grupoAdicional.Maximo,
                              Minimo = grupoAdicional.Minimo,
                              TipoNome = grupoTipo.Tipo ?? string.Empty,
                              TipoID = grupoTipo.ID,
                              ColGrupoAdicionalItem = (from grupoAdicionalItem in context.GrupoAdicionalItem
                                                       join grupoAdicionalItemImagem in context.GrupoAdicionalItemImagem
                                                           on grupoAdicionalItem.ID equals grupoAdicionalItemImagem.GrupoAdicionalItemID
                                                       where grupoAdicionalItem.GrupoAdicionalID == grupoAdicional.ID
                                                             && grupoAdicionalItemImagem.Ativo
                                                             && grupoAdicionalItem.Ativo
                                                       select new GrupoAdicionalItemGetDTO
                                                       {
                                                           ID = grupoAdicionalItem.ID,
                                                           Ativo = grupoAdicionalItem.Ativo,
                                                           DataCadastro = grupoAdicionalItem.DataCadastro,
                                                           Nome = grupoAdicionalItem.Nome,
                                                           ColGrupoAdicionalItemImagem = new ProdutoImagemGetDTO()
                                                           {
                                                               ID = grupoAdicionalItemImagem.ID,
                                                               Arquivo = grupoAdicionalItemImagem.Arquivo,
                                                               Nome = grupoAdicionalItemImagem.Nome,
                                                           },
                                                           Preco = grupoAdicionalItem.Preco,
                                                       }).ToList(),
                              Posicao = context.ProdutoGrupoAdicional
                                  .Where(pga => pga.GrupoAdicionalID == grupoAdicional.ID && pga.ProdutoID == productID)
                                  .Select(pga => (int?)pga.Posicao)
                                  .FirstOrDefault(),
                          })
                          .OrderBy(e => e.Posicao)
                          .ToListAsync();
        }

        public async Task<IEnumerable<GrupoAdicionalGetDTO>> GetAdditionalGroupByEmpresaID(int empresaID)
        {
            return await (from grupoAdicional in context.GrupoAdicional
                          join grupoTipo in context.TipoGrupoAdicional
                              on grupoAdicional.TipoID equals grupoTipo.ID
                          where grupoAdicional.Ativo && grupoAdicional.EmpresaID == empresaID
                          select new GrupoAdicionalGetDTO
                          {
                              ID = grupoAdicional.ID,
                              Nome = grupoAdicional.Nome ?? string.Empty,
                              Ativo = grupoAdicional.Ativo,
                              EmpresaID = grupoAdicional.EmpresaID,
                              Maximo = grupoAdicional.Maximo,
                              Minimo = grupoAdicional.Minimo,
                              TipoNome = grupoTipo.Tipo ?? string.Empty,
                              TipoID = grupoTipo.ID,
                              ColGrupoAdicionalItem = (from grupoAdicionalItem in context.GrupoAdicionalItem
                                                       join grupoAdicionalItemImagem in context.GrupoAdicionalItemImagem
                                                           on grupoAdicionalItem.ID equals grupoAdicionalItemImagem.GrupoAdicionalItemID
                                                       where grupoAdicionalItem.GrupoAdicionalID == grupoAdicional.ID
                                                             && grupoAdicionalItemImagem.Ativo
                                                             && grupoAdicionalItem.Ativo
                                                       select new GrupoAdicionalItemGetDTO
                                                       {
                                                           ID = grupoAdicionalItem.ID,
                                                           Ativo = grupoAdicionalItem.Ativo,
                                                           DataCadastro = grupoAdicionalItem.DataCadastro,
                                                           Nome = grupoAdicionalItem.Nome,
                                                           ColGrupoAdicionalItemImagem = new ProdutoImagemGetDTO()
                                                           {
                                                               ID = grupoAdicionalItemImagem.ID,
                                                               Arquivo = grupoAdicionalItemImagem.Arquivo,
                                                               Nome = grupoAdicionalItemImagem.Nome,
                                                           },
                                                           Preco = grupoAdicionalItem.Preco,
                                                       }).ToList(),
                              Posicao = context.ProdutoGrupoAdicional
                                  .Where(pga => pga.GrupoAdicionalID == grupoAdicional.ID)
                                  .Select(pga => (int?)pga.Posicao)
                                  .FirstOrDefault(),
                          })
                          .OrderBy(e => e.Posicao)
                          .ToListAsync();
        }

        public async Task<IEnumerable<GrupoAdicionalGetDTO>> GetAdditionalGroupWithProductByEmpresaID(int empresaID)
        {
            return await (from grupoAdicional in context.GrupoAdicional
                          join grupoTipo in context.TipoGrupoAdicional on grupoAdicional.TipoID equals grupoTipo.ID
                          where grupoAdicional.Ativo && grupoAdicional.EmpresaID == empresaID
                          select new GrupoAdicionalGetDTO
                          {
                              ID = grupoAdicional.ID,
                              Nome = grupoAdicional.Nome ?? string.Empty,
                              Ativo = grupoAdicional.Ativo,
                              EmpresaID = grupoAdicional.EmpresaID,
                              Maximo = grupoAdicional.Maximo,
                              Minimo = grupoAdicional.Minimo,
                              TipoNome = grupoTipo.Tipo ?? string.Empty,
                              TipoID = grupoTipo.ID,
                              ColGrupoAdicionalItem = (from grupoAdicionalItem in context.GrupoAdicionalItem
                                                       join grupoAdicionalItemImagem in context.GrupoAdicionalItemImagem on grupoAdicionalItem.ID equals grupoAdicionalItemImagem.GrupoAdicionalItemID
                                                       where grupoAdicionalItem.GrupoAdicionalID == grupoAdicional.ID && grupoAdicionalItemImagem.Ativo && grupoAdicionalItem.Ativo
                                                       select new GrupoAdicionalItemGetDTO
                                                       {
                                                           ID = grupoAdicionalItem.ID,
                                                           Ativo = grupoAdicionalItem.Ativo,
                                                           DataCadastro = grupoAdicionalItem.DataCadastro,
                                                           Nome = grupoAdicionalItem.Nome,
                                                           ColGrupoAdicionalItemImagem = new ProdutoImagemGetDTO()
                                                           {
                                                               ID = grupoAdicionalItemImagem.ID,
                                                               Arquivo = grupoAdicionalItemImagem.Arquivo,
                                                               Nome = grupoAdicionalItemImagem.Nome,
                                                           },
                                                           Preco = grupoAdicionalItem.Preco,
                                                       }).ToList(),
                              ColProducts = (from produto in context.Produto
                                             join produtoGrupoAdicional in context.ProdutoGrupoAdicional on produto.ID equals produtoGrupoAdicional.ProdutoID
                                             join categoria in context.Categoria on produto.CategoriaID equals categoria.ID
                                             where produtoGrupoAdicional.GrupoAdicionalID == grupoAdicional.ID && produtoGrupoAdicional.Ativo && produto.Ativo && categoria.Ativo
                                             select new ProdutoGetDTO
                                             {
                                                 Nome = produto.Nome ?? string.Empty,
                                                 Categoria = categoria.Nome ?? string.Empty
                                             }).ToList(),
                          }).OrderBy(e => e.Nome).ToListAsync();
        }

        public async Task AddAsync(GrupoAdicionalItem entity)
        {
            try
            {
                if (entity == null)
                {
                    throw new ArgumentNullException(nameof(entity));
                }
                await context.GrupoAdicionalItem.AddAsync(entity);
            }
            catch
            {
                throw;
            }
        }

        public async Task UpdateAsync(GrupoAdicionalItem categoria)
        {
            categoria.DataEdicao = DateTime.Now;
            await Task.FromResult(0);
        }

        public async Task<GrupoAdicionalItem> GetByIDAsync(int produtoID, int empresaID)
        {
            return null;
        }
    }
}
