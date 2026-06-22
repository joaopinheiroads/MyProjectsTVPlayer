using Cardápio.Dto;
using Cardápio.Infra.Data;
using Cardápio.Infra.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cardápio.Infra.Repositories
{
    public class CidadeRepo : ICidadeRepo
    {
        private readonly AppDbContext context;

        public CidadeRepo(AppDbContext context)
        {
            this.context = context;
        }

        public async Task<IEnumerable<CidadeGetDTO>> GetCidadeByEstadoIDAsync(string estadoID)
        {
            return await (from cidade in context.Cidade
                          where cidade.CidEstadoID == estadoID
                          select new CidadeGetDTO
                          {
                              CidID = cidade.CidID,
                              CidNome = cidade.CidNome
                          }
                          ).ToListAsync();
        }

        public async Task<CidadeGetDTO> GetCidadeByIDAsync(int cidadeID, int empresaID)
        {
            return await (from cidade in context.Cidade
                          where cidade.CidID == cidadeID
                          select new CidadeGetDTO
                          {
                              CidID = cidade.CidID,
                              CidNome = cidade.CidNome
                          }
                          ).FirstOrDefaultAsync();
        }
    }
}
