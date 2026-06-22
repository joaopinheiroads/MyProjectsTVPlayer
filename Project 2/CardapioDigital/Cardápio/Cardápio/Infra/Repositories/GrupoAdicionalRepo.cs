using Cardápio.Infra.Data;
using Cardápio.Infra.Interfaces.Repositories;
using Cardápio.Infra.Model;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace Cardápio.Infra.Repositories
{
    public class GrupoAdicionalRepo : IGrupoAdicionalRepo
    {
        private readonly AppDbContext context;

        public GrupoAdicionalRepo(AppDbContext context)
        {
            this.context = context;
        }

        public async Task<string> GetUniqueGroupName(string baseName, int empresaID, int? grupoID = null)
        {
            Regex regex = new Regex($"^{Regex.Escape(baseName)}\\s?\\((\\d+)\\)$");

            List<string> existingNames = await context.GrupoAdicional
                .Where(g => g.Nome.StartsWith(baseName) && g.Ativo && g.EmpresaID == empresaID)
                .Select(g => g.Nome)
                .ToListAsync();

            if (grupoID != null && existingNames.Contains(baseName))
            {
                return baseName;
            }

            List<int> usedNumbers = existingNames
                .Select(n => regex.Match(n))
                .Where(m => m.Success)
                .Select(m => int.Parse(m.Groups[1].Value))
                .ToList();

            if (existingNames.Contains(baseName))
            {
                int newNumber = usedNumbers.Any() ? usedNumbers.Max() + 1 : 1;
                return $"{baseName} ({newNumber})";
            }

            return baseName;
        }

        public async Task<GrupoAdicional> GetGrupoAdicionalAsyncByIdAndEmpresaID(int grupoID, int empresaID)
        {
            return await context.GrupoAdicional
                .Where(g => g.ID == grupoID && g.EmpresaID == empresaID && g.Ativo)
                .Include(g => g.Tipo)
                .Include(g => g.ProdutoGruposAdicional)
                    .ThenInclude(pg => pg.Produto)
                .Include(g => g.Produtos)
                    .ThenInclude(p => p.ColImagemAdicional) 
                .FirstOrDefaultAsync();
        }

        public async Task<GrupoAdicional> CompararGrupoEBuscarOuCriarNovo(int empresaID, GrupoAdicional groupDB)
        {
            string nomeBase = groupDB.Nome;
            Regex regex = new Regex($@"^{Regex.Escape(nomeBase)}\s?\((\d+)\)$");

            GrupoAdicional grupoExistente = await context.GrupoAdicional
                .Where(g => g.EmpresaID == empresaID
                            && g.Nome.StartsWith(nomeBase)
                            && g.Tipo == groupDB.Tipo
                            && g.Produtos.Count == groupDB.Produtos.Count
                            && g.Ativo)
                .FirstOrDefaultAsync();

            if (grupoExistente != null)
            {
                return grupoExistente;
            }

            List<string> nomesGruposExistentes = await context.GrupoAdicional
                .Where(g => g.EmpresaID == empresaID && g.Ativo && g.Nome.StartsWith(nomeBase))
                .Select(g => g.Nome)
                .ToListAsync();

            List<int> numerosUsados = nomesGruposExistentes
                .Select(n => regex.Match(n))
                .Where(m => m.Success)
                .Select(m => int.Parse(m.Groups[1].Value))
                .ToList();

            int contador = numerosUsados.Any() ? numerosUsados.Max() + 1 : (nomesGruposExistentes.Contains(nomeBase) ? 1 : 0);

            string novoNomeGrupo = contador == 0 ? nomeBase : $"{nomeBase} ({contador})";

            return new GrupoAdicional
            {
                Nome = novoNomeGrupo,
                EmpresaID = empresaID,
                Ativo = groupDB.Ativo,
                Minimo = groupDB.Minimo,
                Maximo = groupDB.Maximo,
                Tipo = groupDB.Tipo,
                TipoID = groupDB.TipoID
            };
        }

        public async Task AddAsync(GrupoAdicional entity)
        {
            try
            {
                if (entity == null)
                {
                    throw new ArgumentNullException(nameof(entity));
                }
                await context.GrupoAdicional.AddAsync(entity);
            }
            catch
            {
                throw;
            }
        }

        public async Task UpdateAsync(GrupoAdicional categoria)
        {
            categoria.DataEdicao = DateTime.Now;
            await Task.FromResult(0);
        }

        public async Task<GrupoAdicional> GetByIDAsync(int produtoID, int empresaID)
        {
            return null;
        }
    }
}
