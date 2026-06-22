using Cardápio.Dto;

namespace Cardápio.Infra.Model
{
    public class PedidoItemAddDTO
    {
        public int ProdutoID { get; set; }
        public string Nome { get; set; }
        public int Quantidade { get; set; }
        public string Observacao { get; set; }
        public decimal Preco { get; set; }
        public decimal Subtotal { get; set; }
        public List<PedidoItemAdicionalAddDTO> ColPedidoItemAdicional { get; set; }
    }
}
