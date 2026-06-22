using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Cardápio.Infra.Model
{
    public class PedidoStatus
    {
        public int ID { get; set; }

        [Column(TypeName = "varchar")]
        [MaxLength(100)]
        public string Status { get; set; }
    }
}
