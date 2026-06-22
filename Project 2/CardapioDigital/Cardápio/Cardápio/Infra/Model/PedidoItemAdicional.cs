using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Cardápio.Infra.Model
{
    public class PedidoItemAdicional
    {
        public int ID { get; set; }

        [Required]
        public int PedidoItemID { get; set; }

        [ForeignKey(nameof(PedidoItemID))]
        public PedidoItem PedidoItem { get; set; }

        [Required]
        public int GrupoAdicionalItemID { get; set; }

        [ForeignKey(nameof(GrupoAdicionalItemID))]
        public PedidoItem GrupoAdicionalItem { get; set; }

        [Required]
        public int Quantidade { get; set; }

        [Required]
        public int PrecoUnitario { get; set; }

        [Required]
        public int Subtotal { get; set; }
    }
}
