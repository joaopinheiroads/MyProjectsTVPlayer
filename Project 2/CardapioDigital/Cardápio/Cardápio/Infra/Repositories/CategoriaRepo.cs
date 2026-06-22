using Cardápio.Dto;
using Cardápio.Infra.Data;
using Cardápio.Infra.Interfaces.Repositories;
using Cardápio.Infra.Model;
using Microsoft.EntityFrameworkCore;

namespace Cardápio.Infra.Repositories
{
    public class CategoriaRepo : ICategoriaRepo
    {
        private readonly AppDbContext context;

        public CategoriaRepo(AppDbContext context)
        {
            this.context = context;
        }

        public async Task AddAsync(Category entity)
        {
            try
            {
                if (entity == null)
                {
                    throw new ArgumentNullException(nameof(entity));
                }
                await context.Categoria.AddAsync(entity);
            }
            catch
            {
                throw;
            }
        }

        public async Task UpdateAsync(Category categoria)
        {
            categoria.DataEdicao = DateTime.Now;
            await Task.FromResult(0);
        }

        public async Task<Category> GetByIDAsync(int categoriaID, int empresaID)
        {
            return await (from categoria in context.Categoria
                          where categoria.ID == categoriaID &&
                                categoria.Ativo == true
                          select categoria
                        ).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<CategoriaGetDTO>> GetCategoriaByEmpresaIDAsync(int empresaID, int usuarioID, int grupoID, bool isAdmin)
        {
            return await (from categoria in context.Categoria
                          join usuario in context.Usuario on categoria.UsuarioIDCadastro equals usuario.ID
                          join grupoUsuario in context.GrupoUsuario on usuario.ID equals grupoUsuario.UsuarioID into grupoUsuarioGroup
                          from grupoUsuario in grupoUsuarioGroup.DefaultIfEmpty()
                          where categoria.Ativo
                          && (
                              isAdmin && grupoUsuario != null && grupoUsuario.GrupoID == grupoID
                              || categoria.EmpresaID == empresaID
                              )
                          select new CategoriaGetDTO
                          {
                              ID = categoria.ID,
                              Nome = categoria.Nome,
                              Ordem = categoria.Ordem,
                              DataCadastro = categoria.DataCadastro,
                              BackgroundColor = categoria.BackgroundCor
                          }).OrderBy(c => c.Ordem).ToListAsync();
        }

        public async Task<IEnumerable<CategoriaGetDTO>> GetCategoriaByEmpresaIDAsyncNotAuthenticated(int empresaID)
        {
            return await (from categoria in context.Categoria
                          where categoria.Ativo && categoria.EmpresaID == empresaID
                          select new CategoriaGetDTO
                          {
                              ID = categoria.ID,
                              Nome = categoria.Nome,
                              Ordem = categoria.Ordem,
                              DataCadastro = categoria.DataCadastro,
                              BackgroundColor = categoria.BackgroundCor ?? "null"
                          }).OrderBy(c => c.Ordem).ToListAsync();
        }

        //duplicado
        public async Task<IEnumerable<CategoriaGetDTO>> GetCategoriaByEmpresaIDAsyncNotAuthenticated(string empresaNome)
        {
            string normalizedEmpresaNome = empresaNome.Replace("-", " ").Trim();

            return await (from categoria in context.Categoria
                          where categoria.Ativo && categoria.Empresa.Nome.Replace("-", "").Replace(" ", "").ToLower() ==
                                normalizedEmpresaNome.Replace("-", "").Replace(" ", "").ToLower()
                          select new CategoriaGetDTO
                          {
                              ID = categoria.ID,
                              Nome = categoria.Nome,
                              Ordem = categoria.Ordem,
                              DataCadastro = categoria.DataCadastro,
                              BackgroundColor = categoria.BackgroundCor ?? "null"
                          }).OrderBy(c => c.Ordem).ToListAsync();
        }

        public async Task<ICollection<Category>> GetByListCategoriaIDAsync(List<int> listCategoriaID, int empresaID)
        {
            return await (from categoria in context.Categoria
                          where listCategoriaID.Contains(categoria.ID)
                          select categoria).ToListAsync();
        }

        public async Task<ICollection<Category>> GetByBiggerThenOrdemAsync(int ordem, int empresaID)
        {
            return await (from categoria in context.Categoria
                          join empresa in context.Empresa on categoria.EmpresaID equals empresa.ID
                          where categoria.Ordem > ordem &&
                                empresa.Ativo == true &&
                                categoria.EmpresaID == empresaID
                          select categoria).ToListAsync();
        }


        public async Task<CategoriaGetDTO> GetCategoriaByIDAsync(int categoriaID, int empresaID)
        {
            return await (from categoria in context.Categoria
                          join empresa in context.Empresa on categoria.EmpresaID equals empresa.ID
                          where categoria.ID == categoriaID &&
                                categoria.Ativo &&
                                empresa.Ativo &&
                                categoria.EmpresaID == empresaID
                          select new CategoriaGetDTO
                          {
                              ID = categoria.ID,
                              Nome = categoria.Nome,
                              Ordem = categoria.Ordem
                          }).FirstOrDefaultAsync();
        }

        public async Task<int> GetNextOrdemByEmpresaIDAsync(int empresaID)
        {
            return await (from categoria in context.Categoria
                          join empresa in context.Empresa on categoria.EmpresaID equals empresa.ID
                          where categoria.Ativo &&
                                empresa.Ativo &&
                                categoria.EmpresaID == empresaID
                          select categoria
                          ).CountAsync() + 1;
        }

        public async Task UpdateRangeAsync(ICollection<Category> categoriasExistentes)
        {
            if (categoriasExistentes == null || !categoriasExistentes.Any())
            {
                throw new ArgumentException("A coleção de categorias não pode ser nula ou vazia.", nameof(categoriasExistentes));
            }

            var ids = categoriasExistentes.Select(c => c.ID).ToList();

            var categoriasNoBanco = await context.Categoria
                .Where(c => ids.Contains(c.ID))
                .ToListAsync();

            foreach (var categoria in categoriasNoBanco)
            {
                var categoriaAtualizada = categoriasExistentes.First(c => c.ID == categoria.ID);
                categoria.Ordem = categoriaAtualizada.Ordem;
                categoria.UsuarioIDEdicao = categoriaAtualizada.UsuarioIDEdicao;
            }

            context.Categoria.UpdateRange(categoriasNoBanco);

            await context.SaveChangesAsync();
        }
    }
}
