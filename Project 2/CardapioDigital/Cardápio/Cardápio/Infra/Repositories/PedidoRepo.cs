using Cardápio.Dto;
using Cardápio.Infra.Data;
using Cardápio.Infra.Interfaces.Repositories;
using Cardápio.Infra.Model;
using Microsoft.EntityFrameworkCore;

namespace Cardápio.Infra.Repositories
{
    public class PedidoRepo : IPedidoRepo
    {
        private readonly AppDbContext context;

        public PedidoRepo(AppDbContext context)
        {
            this.context = context;
        }

        public async Task AddAsync(Pedido pedido)
        {
            try
            {
                if (pedido == null)
                {
                    throw new ArgumentNullException(nameof(pedido));
                }
                await context.Pedido.AddAsync(pedido);
            }
            catch
            {
                throw;
            }
        }

        public async Task UpdateAsync(Pedido pedido)
        {
            pedido.DataEdicao = DateTime.Now;
            await Task.FromResult(0);
        }

        public async Task<Pedido> GetByIDAsync(int pedidoID, int empresaID)
        {
            return await (from pedido in context.Pedido
                          where pedido.ID == pedidoID &&
                                pedido.Ativo == true
                          select pedido
                        ).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<TodosPedidoGetDTO>> GetDTOByEmpresaID(int empresaID)
        {
            return await (from pedido in context.Pedido
                          where pedido.EmpresaID == empresaID &&
                          pedido.Ativo == true
                          select new TodosPedidoGetDTO()
                          {
                              PedidoStatus = pedido.PedidoStatus.Status,
                              DataPedidoCriado = pedido.DataCadastro,
                              NomeMesa = pedido.Mesa.NomeMesa,
                              ValorTotal = pedido.Total,
                              UsuarioClienteNome = pedido.UsuarioCliente.Nome,
                          }).ToListAsync();
        }
    }
}
