using Cardápio.Dto;
using Cardápio.Infra.Data;
using Cardápio.Infra.Interfaces.Repositories;
using Cardápio.Infra.Model;
using Microsoft.EntityFrameworkCore;

namespace Cardápio.Infra.Repositories
{
    public class MesaRepo : IMesaRepo
    {
        private readonly AppDbContext context;

        public MesaRepo(AppDbContext context)
        {
            this.context = context;
        }

        public async Task AddAsync(Mesa entity)
        {
            try
            {
                if (entity == null)
                {
                    throw new ArgumentNullException(nameof(entity));
                }
                await context.Mesa.AddAsync(entity);
            }
            catch
            {
                throw;
            }
        }

        public async Task UpdateAsync(Mesa categoria)
        {
            categoria.DataEdicao = DateTime.Now;
            await Task.FromResult(0);
        }

        public async Task<Mesa> GetByIDAsync(int mesaID, int empresaID)
        {
            return await (from mesa in context.Mesa
                          where mesa.ID == mesaID &&
                                mesa.EmpresaID == empresaID &&
                                mesa.Ativo
                          select mesa
                        ).FirstOrDefaultAsync();
        }

        public async Task<List<MesaGetDTO>> GetAllMesas(int empresaID)
        {
            return await (from mesa in context.Mesa
                          join mesaStatus in context.MesaStatus
                          on mesa.MesaStatusID equals mesaStatus.ID
                          where mesa.EmpresaID == empresaID && mesa.Ativo
                          select new MesaGetDTO
                          {
                              ID = mesa.ID,
                              MesaStatus = mesaStatus.State,
                              QrCode = mesa.QRCode,
                              NomeMesa = mesa.NomeMesa,
                              DataCadastro = mesa.DataCadastro,
                          }).ToListAsync();
        }

        public async Task<int> CountMesasAtivas(int empresaID)
        {
            return await (from mesa in context.Mesa
                          where mesa.EmpresaID == empresaID && mesa.Ativo
                          select mesa).CountAsync();
        }
    }
}
