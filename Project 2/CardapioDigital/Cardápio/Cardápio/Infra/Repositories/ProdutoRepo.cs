using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using Cardápio.Dto;
using Cardápio.Infra.Model;
using Cardápio.Infra.Interfaces.Repositories;
using Cardápio.Infra.Data;
using Cardápio.Client.Pages.Produto;
using SkiaSharp;

namespace Cardápio.Infra.Repositories
{
    public class ProdutoRepo : IProdutoRepo
    {
        private readonly AppDbContext context;

        public ProdutoRepo(AppDbContext context)
        {
            this.context = context;
        }

        public async Task AddAsync(Product entity)
        {
            try
            {
                if (entity == null)
                {
                    throw new ArgumentNullException(nameof(entity));
                }
                await context.Produto.AddAsync(entity);
            }
            catch
            {
                throw;
            }
        }
        public async Task<Product> GetByIDAsync(int produtoID, int empresaID)
        {
            return await (from produto in context.Produto.Include(produto => produto.ColProdutoImagem.Where(produtoImagem => produtoImagem.Ativo == true)).Include(t => t.ProdutoThumbnail).Include(pg => pg.ProdutoGruposAdicional)
                          where produto.ID == produtoID &&
                                  produto.Excluido == false
                          select produto
                              ).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<ProdutoGetDTO>> GetProdutoByQRCodeAsync(string qrCode)
        {

            return await (from produto in context.Produto
                          join empresa in context.Empresa on produto.EmpresaID equals empresa.ID
                          where produto.Ativo == true &&
                                empresa.Ativo == true &&
                                empresa.QRCode == qrCode
                          select new ProdutoGetDTO
                          {
                              ID = produto.ID,
                              Nome = produto.Nome,
                              Descricao = produto.Descricao,
                              Preco = produto.Preco,
                              CategoriaID = produto.CategoriaID,
                              Categoria = produto.Categoria.Nome,
                              ImagesID = ConvertProdutoImagem(produto.ColProdutoImagem.Where(i => i.Ativo == true).ToList()),
                              ImagemThumbnail = produto.ProdutoThumbnail != null ? produto.ProdutoThumbnail.Arquivo : "Default.png",
                              PrecoPromocional = produto.PrecoPromocional,
                              Promocao = produto.Promocao,
                              FimPromocao = produto.FimPromocao,
                              Destaque = produto.Destaque,
                              CategoriaOrdem = produto.Categoria.Ordem
                          }).OrderBy(p => p.CategoriaOrdem).ToListAsync();
        }

        public async Task<IEnumerable<ProdutoGetDTO>> GetProdutoByGrupoAdicionalID(int grupoID)
        {
            return await (from produto in context.Produto
                          join produtoGrupoAdicional in context.ProdutoGrupoAdicional
                          on produto.ID equals produtoGrupoAdicional.ProdutoID
                          where produtoGrupoAdicional.GrupoAdicionalID == grupoID && produtoGrupoAdicional.Ativo && produto.Ativo
                          select new ProdutoGetDTO
                          {
                              ID = produto.ID,
                              Nome = produto.Nome,
                              Preco = produto.Preco,
                              ColGrupoAdicional = (
                                  from produtoGrupoAdicional in context.ProdutoGrupoAdicional
                                  join grupoAdicional in context.GrupoAdicional on produtoGrupoAdicional.GrupoAdicionalID equals grupoAdicional.ID
                                  where produtoGrupoAdicional.ProdutoID == produto.ID && produtoGrupoAdicional.Ativo == true
                                  select new GrupoAdicionalGetDTO
                                  {
                                      ID = grupoAdicional.ID,
                                      Nome = grupoAdicional.Nome,
                                      Ativo = grupoAdicional.Ativo,
                                      EmpresaID = grupoAdicional.EmpresaID,
                                      Minimo = grupoAdicional.Minimo,
                                      Maximo = grupoAdicional.Maximo,
                                      TipoNome = grupoAdicional.Tipo.Tipo,
                                      TipoID = grupoAdicional.TipoID,
                                      Posicao = produtoGrupoAdicional.Posicao,
                                      ColGrupoAdicionalItem = (
                                        from adicionalItem in context.GrupoAdicionalItem
                                        where adicionalItem.GrupoAdicionalID == grupoAdicional.ID
                                        select new GrupoAdicionalItemGetDTO
                                        {
                                            ID = adicionalItem.ID,
                                            Nome = adicionalItem.Nome,
                                            Preco = adicionalItem.Preco,
                                            Ativo = adicionalItem.Ativo,
                                            ColGrupoAdicionalItemImagem = (
                                                from imagem in context.GrupoAdicionalItemImagem
                                                where imagem.GrupoAdicionalItemID == adicionalItem.ID
                                                select new ProdutoImagemGetDTO
                                                {
                                                    ID = imagem.ID,
                                                    Nome = imagem.Nome,
                                                    Arquivo = imagem.Arquivo
                                                }
                                            ).FirstOrDefault()
                                        }
                                    ).ToList()
                                  }
                              ).ToList()
                          }).ToListAsync();
        }

        private static Collection<string> ConvertProdutoImagem(ICollection<ImageProduct> colProdutoImagem)
        {
            Collection<string> colProdutoImagemGetDTO = new();

            if (colProdutoImagem != null)
            {
                foreach (ImageProduct produtoImagem in colProdutoImagem)
                {
                    if (!string.IsNullOrWhiteSpace(produtoImagem.Nome))
                    {
                        colProdutoImagemGetDTO.Add(produtoImagem.Nome);
                    }
                }
            }

            // Se não há imagens, adiciona uma imagem padrão
            if (colProdutoImagemGetDTO.Count == 0)
            {
                colProdutoImagemGetDTO.Add("Default.png");
            }

            return colProdutoImagemGetDTO;
        }

        public async Task<IEnumerable<ProdutoGetDTO>> GetProdutoByEmpresaNomeAsync(string empresaNome)
        {
            string normalizedEmpresaNome = empresaNome.Replace("-", " ").Trim();

            return await (from produto in context.Produto
                          join usuario in context.Usuario on produto.UsuarioIDCadastro equals usuario.ID
                          join grupoUsuario in context.GrupoUsuario on usuario.ID equals grupoUsuario.UsuarioID into grupoUsuarioGroup
                          from grupoUsuario in grupoUsuarioGroup.DefaultIfEmpty()
                          where produto.Ativo
                                && produto.Empresa.Nome.Replace("-", "").Replace(" ", "").ToLower() == normalizedEmpresaNome.Replace("-", "").Replace(" ", "").ToLower()
                                && produto.Excluido == false
                          select new ProdutoGetDTO
                          {
                              ID = produto.ID,
                              Nome = produto.Nome,
                              Descricao = produto.Descricao,
                              Preco = produto.Preco,
                              QTDPessoa = produto.QTDPessoa,
                              CategoriaID = produto.CategoriaID,
                              Categoria = produto.Categoria.Nome,
                              ImagemThumbnail = produto.ProdutoThumbnail != null ? produto.ProdutoThumbnail.Arquivo : "Default.png",
                              PrecoPromocional = produto.PrecoPromocional,
                              Promocao = produto.Promocao,
                              FimPromocao = produto.FimPromocao,
                              Destaque = produto.Destaque,
                              CategoriaOrdem = produto.Categoria.Ordem,
                              DataCadastro = produto.DataCadastro,
                              ImagesID = ConvertProdutoImagem(produto.ColProdutoImagem.Where(i => i.Ativo == true).ToList()),
                              // Adicionar PromocoesPorHorario aqui também
                              PromocoesPorHorario = produto.ProdutoPromocaoHorarios
                                  .Where(ph => ph.Ativo == true)
                                  .Select(ph => new ProdutoPromocaoHorarioGetDTO
                                  {
                                      ID = ph.ID,
                                      DiaSemana = ph.DiaSemana,
                                      HoraInicio = ph.HoraInicio.ToString(@"hh\:mm"),
                                      HoraFim = ph.HoraFim.ToString(@"hh\:mm"),
                                      PrecoPromocional = ph.PrecoPromocional,
                                      Ativo = ph.Ativo,
                                      DataCadastro = ph.DataCadastro
                                  }).ToList(),
                              ColGrupoAdicional = (
                                  from produtoGrupoAdicional in context.ProdutoGrupoAdicional
                                  join grupoAdicional in context.GrupoAdicional on produtoGrupoAdicional.GrupoAdicionalID equals grupoAdicional.ID
                                  where produtoGrupoAdicional.ProdutoID == produto.ID && produtoGrupoAdicional.Ativo == true
                                  select new GrupoAdicionalGetDTO
                                  {
                                      ID = grupoAdicional.ID,
                                      Nome = grupoAdicional.Nome,
                                      Ativo = grupoAdicional.Ativo,
                                      EmpresaID = grupoAdicional.EmpresaID,
                                      Minimo = grupoAdicional.Minimo,
                                      Maximo = grupoAdicional.Maximo,
                                      TipoNome = grupoAdicional.Tipo.Tipo,
                                      TipoID = grupoAdicional.TipoID,
                                      Posicao = produtoGrupoAdicional.Posicao,
                                      ColGrupoAdicionalItem = (
                                        from adicionalItem in context.GrupoAdicionalItem
                                        where adicionalItem.GrupoAdicionalID == grupoAdicional.ID
                                        select new GrupoAdicionalItemGetDTO
                                        {
                                            ID = adicionalItem.ID,
                                            Nome = adicionalItem.Nome,
                                            Preco = adicionalItem.Preco,
                                            Ativo = adicionalItem.Ativo,
                                            ColGrupoAdicionalItemImagem = (
                                                from imagem in context.GrupoAdicionalItemImagem
                                                where imagem.GrupoAdicionalItemID == adicionalItem.ID
                                                select new ProdutoImagemGetDTO
                                                {
                                                    ID = imagem.ID,
                                                    Nome = imagem.Nome,
                                                    Arquivo = imagem.Arquivo
                                                }
                                            ).FirstOrDefault()
                                        }
                                    ).ToList()
                                  }
                              ).ToList()
                          }).OrderBy(p => p.CategoriaOrdem).ToListAsync();
        }

        public async Task<IEnumerable<ProdutoGetDTO>> GetProdutoByGrupoID(int grupoID)
        {
            return await (from produto in context.Produto
                                  join pga in context.ProdutoGrupoAdicional
                                  on produto.ID equals pga.ProdutoID
                                  join categoria in context.Categoria
                                  on produto.CategoriaID equals categoria.ID
                                  where pga.GrupoAdicionalID == grupoID &&
                                        pga.Ativo && produto.Ativo
                                  select new ProdutoGetDTO
                                  {
                                      ID = produto.ID,
                                      Nome = produto.Nome,
                                      Categoria = categoria.Nome
                                  }).ToListAsync();
        }

        public async Task<IEnumerable<ProdutoGetDTO>> GetProdutoByEmpresaIdAsync(int empresaID, int usuarioID, int grupoID, bool isAdmin)
        {
            return await (from produto in context.Produto
                          .Include(p => p.ProdutoThumbnail)
                          .Include(p => p.ColProdutoImagem.Where(i => i.Ativo == true))
                          .Include(p => p.Categoria)
                          join usuario in context.Usuario on produto.UsuarioIDCadastro equals usuario.ID
                          join grupoUsuario in context.GrupoUsuario on usuario.ID equals grupoUsuario.UsuarioID into grupoUsuarioGroup
                          from grupoUsuario in grupoUsuarioGroup.DefaultIfEmpty()
                          where (
                                    isAdmin && grupoUsuario != null && grupoUsuario.GrupoID == grupoID
                                    ||
                                    !isAdmin && grupoUsuario != null && grupoUsuario.GrupoID == grupoID
                                )
                                && produto.EmpresaID == empresaID
                                && produto.Excluido == false
                          select new ProdutoGetDTO
                          {
                              ID = produto.ID,
                              Nome = produto.Nome,
                              Descricao = produto.Descricao,
                              Preco = produto.Preco,
                              QTDPessoa = produto.QTDPessoa,
                              CategoriaID = produto.CategoriaID,
                              Categoria = produto.Categoria.Nome,
                              ImagemThumbnail = produto.ProdutoThumbnail != null ? produto.ProdutoThumbnail.Arquivo : "Default.png",
                              PrecoPromocional = produto.PrecoPromocional,
                              Promocao = produto.Promocao,
                              FimPromocao = produto.FimPromocao,
                              Destaque = produto.Destaque,
                              CategoriaOrdem = produto.Categoria.Ordem,
                              DataCadastro = produto.DataCadastro,
                              Ativo = produto.Ativo,
                              ImagesID = ConvertProdutoImagem(produto.ColProdutoImagem.Where(i => i.Ativo == true).ToList()),
                              ColGrupoAdicional = (
                                  from produtoGrupoAdicional in context.ProdutoGrupoAdicional
                                  join grupoAdicional in context.GrupoAdicional on produtoGrupoAdicional.GrupoAdicionalID equals grupoAdicional.ID
                                  where produtoGrupoAdicional.ProdutoID == produto.ID && produtoGrupoAdicional.Ativo == true
                                  select new GrupoAdicionalGetDTO
                                  {
                                      ID = grupoAdicional.ID,
                                      Nome = grupoAdicional.Nome,
                                      Ativo = grupoAdicional.Ativo,
                                      EmpresaID = grupoAdicional.EmpresaID,
                                      Minimo = grupoAdicional.Minimo,
                                      Maximo = grupoAdicional.Maximo,
                                      TipoNome = grupoAdicional.Tipo.Tipo,
                                      TipoID = grupoAdicional.TipoID,
                                      Posicao = produtoGrupoAdicional.Posicao,
                                      ColGrupoAdicionalItem = (
                                        from adicionalItem in context.GrupoAdicionalItem
                                        where adicionalItem.GrupoAdicionalID == grupoAdicional.ID
                                        select new GrupoAdicionalItemGetDTO
                                        {
                                            ID = adicionalItem.ID,
                                            Nome = adicionalItem.Nome,
                                            Preco = adicionalItem.Preco,
                                            Ativo = adicionalItem.Ativo,
                                            ColGrupoAdicionalItemImagem = (
                                                from imagem in context.GrupoAdicionalItemImagem
                                                where imagem.GrupoAdicionalItemID == adicionalItem.ID
                                                select new ProdutoImagemGetDTO
                                                {
                                                    ID = imagem.ID,
                                                    Nome = imagem.Nome,
                                                    Arquivo = imagem.Arquivo
                                                }
                                            ).FirstOrDefault()
                                        }
                                    ).ToList()
                                  }
                              ).ToList()
                          }).OrderBy(p => p.Nome).ToListAsync();
        }


        public async Task<ProdutoGetDTO> GetProdutoByIDAsync(int produtoID, int empresaID)
        {
            
            var produto = await context.Produto
                .Include(p => p.Categoria)
                .Include(p => p.ColProdutoImagem.Where(i => i.Ativo == true))
                .Include(p => p.ProdutoThumbnail)
                .Include(p => p.ProdutoHorarios.Where(h => h.Ativo == true))
                .Include(p => p.ProdutoPromocaoHorarios.Where(ph => ph.Ativo == true))
                // 🔧 FIX: Adicionar Include dos grupos adicionais
                .Include(p => p.ProdutoGruposAdicional.Where(pga => pga.Ativo == true))
                    .ThenInclude(pga => pga.GrupoAdicional)
                        .ThenInclude(ga => ga.Tipo)
                .Include(p => p.ProdutoGruposAdicional.Where(pga => pga.Ativo == true))
                    .ThenInclude(pga => pga.GrupoAdicional)
                        .ThenInclude(ga => ga.Produtos.Where(gai => gai.Ativo == true))
                            .ThenInclude(gai => gai.ColImagemAdicional.Where(gaii => gaii.Ativo == true))
                .Where(p => p.ID == produtoID && 
                           p.Excluido == false && 
                           p.EmpresaID == empresaID)
                .FirstOrDefaultAsync();

            if (produto != null)
            {
                if (produto.ProdutoGruposAdicional?.Any() == true)
                {
                    foreach (var pga in produto.ProdutoGruposAdicional)
                    {
                        Console.WriteLine($"[GetProdutoByIDAsync] 📦 Grupo: {pga.GrupoAdicional?.Nome ?? "NULL"} (ID: {pga.GrupoAdicionalID})");
                    }
                }
            }

            if (produto == null)
                return null;

            return new ProdutoGetDTO
            {
                ID = produto.ID,
                Nome = produto.Nome,
                Descricao = produto.Descricao,
                Preco = produto.Preco,
                CategoriaID = produto.CategoriaID,
                Categoria = produto.Categoria.Nome,
                ImagesID = ConvertProdutoImagem(produto.ColProdutoImagem.ToList()),
                ImagemThumbnail = produto.ProdutoThumbnail != null ? produto.ProdutoThumbnail.Arquivo : "Default.png",
                PrecoPromocional = produto.PrecoPromocional,
                Promocao = produto.Promocao,
                FimPromocao = produto.FimPromocao,
                Destaque = produto.Destaque,
                CategoriaOrdem = produto.Categoria.Ordem,
                QTDPessoa = produto.QTDPessoa,
                Ativo = produto.Ativo,
                DataCadastro = produto.DataCadastro,
                Horarios = produto.ProdutoHorarios.Select(h => new ProdutoHorarioGetDTO
                {
                    ID = h.ID,
                    ProdutoID = h.ProdutoID,
                    HoraInicio = h.HoraInicio,
                    HoraFim = h.HoraFim,
                    DiaSemana = h.DiaSemana,
                    Ativo = h.Ativo,
                    DataCadastro = h.DataCadastro
                }).ToList(),
                PromocoesPorHorario = produto.ProdutoPromocaoHorarios.Select(ph => new ProdutoPromocaoHorarioGetDTO
                {
                    ID = ph.ID,
                    DiaSemana = ph.DiaSemana,
                    HoraInicio = ph.HoraInicioString,
                    HoraFim = ph.HoraFimString,
                    PrecoPromocional = ph.PrecoPromocional,
                    Ativo = ph.Ativo,
                    DataCadastro = ph.DataCadastro
                }).ToList(),
                // 🔧 FIX: Adicionar mapeamento dos grupos adicionais
                ColGrupoAdicional = produto.ProdutoGruposAdicional
                    ?.Where(pga => pga.Ativo == true)
                    .OrderBy(pga => pga.Posicao)
                    .Select(pga => 
                    {
                        Console.WriteLine($"[GetProdutoByIDAsync] 🔧 Mapeando grupo: {pga.GrupoAdicional?.Nome ?? "NULL"} (ID: {pga.GrupoAdicionalID})");
                        return new GrupoAdicionalGetDTO
                        {
                            ID = pga.GrupoAdicional.ID,
                        Nome = pga.GrupoAdicional.Nome,
                        Ativo = pga.GrupoAdicional.Ativo,
                        EmpresaID = pga.GrupoAdicional.EmpresaID,
                        Minimo = pga.GrupoAdicional.Minimo,
                        Maximo = pga.GrupoAdicional.Maximo,
                        TipoNome = pga.GrupoAdicional.Tipo.Tipo,
                        TipoID = pga.GrupoAdicional.TipoID,
                        Posicao = pga.Posicao,
                        ColGrupoAdicionalItem = pga.GrupoAdicional.Produtos
                            .Where(gai => gai.Ativo == true)
                            .Select(gai => new GrupoAdicionalItemGetDTO
                            {
                                ID = gai.ID,
                                Nome = gai.Nome,
                                Preco = gai.Preco,
                                Ativo = gai.Ativo,
                                DataCadastro = gai.DataCadastro,
                                ColGrupoAdicionalItemImagem = new ProdutoImagemGetDTO
                                {
                                    Arquivo = gai.ColImagemAdicional
                                        .Where(gaii => gaii.Ativo == true)
                                        .Select(gaii => gaii.Arquivo)
                                        .FirstOrDefault() ?? string.Empty
                                }
                            }).ToList()
                        };
                    }).ToList() ?? new List<GrupoAdicionalGetDTO>()
            };
        }

        public async Task UpdateAsync(Product produto)
        {
            produto.DataEdicao = DateTime.Now;

            await Task.FromResult(0);
        }
    }
}
