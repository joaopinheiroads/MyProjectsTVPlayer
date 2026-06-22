using Cardápio.Dto;
using Microsoft.AspNetCore.SignalR;

namespace Cardápio.Hubs.PedidoHub
{
    public class PedidoHub : Hub
    {
        public async Task EnviarPedido(PedidoAddDTO pedido)
        {

            await Clients.Caller.SendAsync("PedidoConfirmado", pedido);
        }
    }
}
