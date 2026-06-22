using Cardápio.Dto;
using Cardápio.Infra.Data;
using Cardápio.Infra.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cardápio.Infra.Repositories
{
    public class EstadoRepo : IEstadoRepo
    {
        private readonly AppDbContext context;

        public EstadoRepo(AppDbContext context)
        {
            this.context = context;
        }

        public async Task<IEnumerable<EstadoGetDTO>> GetEstadoByEmpresaIDAsync(int empresaID)
        {
            return await (from estado in context.Estado
                          select new EstadoGetDTO
                          {
                              EndEstadoID = estado.EstID
                          }
                          ).ToListAsync();
        }

        public async Task<EstadoGetDTO> GetEstadoByIDAsync(string estadoID, int empresaID)
        {
            return await (from estado in context.Estado
                          where estado.EstID == estadoID
                          select new EstadoGetDTO
                          {
                              EndEstadoID = estado.EstID
                          }
                          ).FirstOrDefaultAsync();
        }
    }
}
