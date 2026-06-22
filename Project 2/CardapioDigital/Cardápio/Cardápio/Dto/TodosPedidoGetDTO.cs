using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Cardápio.Dto
{
    public class TodosPedidoGetDTO
    {
        public string UsuarioClienteNome { get; set; }
        [Required]
        [Column(TypeName = "decimal(19,2)")]
        public decimal ValorTotal { get; set; }
        public string PedidoStatus {  get; set; }
        public string? NomeMesa { get; set; }
        public DateTime DataPedidoCriado { get; set; }
    }
}
