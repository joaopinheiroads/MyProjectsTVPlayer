using Cardápio.Client.Dto;

namespace Cardápio.Client.Pages.ConfirmarPedido
{
    public class PedidoStorage
    {
        private static PedidoAddDTO _pedidoAtual;

        public void SetPedido(PedidoAddDTO pedido)
        {
            _pedidoAtual = pedido;
        }

        public PedidoAddDTO GetPedido()
        {
            return _pedidoAtual;
        }

        public void ClearPedido()
        {
            _pedidoAtual = null;
        }
    }
}
