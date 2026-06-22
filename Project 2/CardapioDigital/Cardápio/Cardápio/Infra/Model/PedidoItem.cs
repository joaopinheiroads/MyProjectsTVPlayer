using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Cardápio.Infra.Model
{
    public class PedidoItem
    {
        public int ID { get; set; }

        [Required]
        public int PedidoID { get; set; }

        [ForeignKey(nameof(PedidoID))]
        public Pedido Pedido { get; set; }

        [Required]
        public int ProdutoID { get; set; }
        [ForeignKey(nameof(ProdutoID))]
        public Pedido Produto { get; set; }

        [Required]
        public int Quantidade { get; set; }

        [Required]
        [Column(TypeName = "decimal(19,2)")]
        public decimal Preco { get; set; }

        [Required]
        [Column(TypeName = "decimal(19,2)")]
        public decimal Subtotal { get; set; }

        public ICollection<PedidoItemAdicional> ColPedidoItemAdicional { get; set; }
    }
}
