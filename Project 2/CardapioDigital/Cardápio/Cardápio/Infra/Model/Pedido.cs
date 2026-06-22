using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Cardápio.Infra.Model
{
    public class Pedido
    {
        public int ID { get; set; }

        [Required]
        public int StatusPedidoID { get; set; }

        [ForeignKey(nameof(StatusPedidoID))]
        public PedidoStatus PedidoStatus { get; set; }

        [Required]
        public int UsuarioClienteID { get; set; }

        [ForeignKey(nameof(UsuarioClienteID))]
        public UsuarioCliente UsuarioCliente { get; set; }

        [Required]
        public int EmpresaID { get; set; }

        [ForeignKey(nameof(EmpresaID))]
        public Company Empresa { get; set; }

        [Required]
        public int MesaID { get; set; }

        [ForeignKey(nameof(MesaID))]
        public Mesa Mesa { get; set; }

        public bool Ativo { get; set; }

        [Required]
        [Column(TypeName = "decimal(19,2)")]
        public decimal Total { get; set; }

        [Column(TypeName = "datetime2(0)")]
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime DataCadastro { get; set; }

        [Column(TypeName = "datetime2(0)")]
        public DateTime? DataEdicao { get; set; }

        public ICollection<PedidoItem> ColPedidoItem { get; set; }
    }
}
