using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace Cardápio.Infra.Model
{
    public class UsuarioCliente
    {
        public int ID { get; set; }

        [Column(TypeName = "varchar")]
        [MaxLength(100)]
        public string? Nome { get; set; }

        [Column(TypeName = "varchar")]
        [MaxLength(100)]
        [EmailAddress]
        public string? Email { get; set; }
        public string? Celular { get; set; }

        public string? CPF { get; set; }

        [Column(TypeName = "datetime2(0)")]
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime DataCadastro { get; set; }

        [Column(TypeName = "datetime2(0)")]
        public DateTime? DataEdicao { get; set; }

        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [DefaultValue(true)]
        public bool Ativo { get; set; }

        [Required]
        public int? UsuarioTipoID { get; set; }

        [ForeignKey(nameof(UsuarioTipoID))]
        public UsuarioTipo UsuarioTipo { get; set; }

        [Required]
        public int? UsuarioIDCadastro { get; set; }

        [ForeignKey(nameof(UsuarioIDCadastro))]
        public UsuarioCliente UsuarioCadastro { get; set; }

        public int? UsuarioIDEdicao { get; set; }

        [ForeignKey(nameof(UsuarioIDEdicao))]
        public UsuarioCliente UsuarioEdicao { get; set; }

        public bool Excluido { get; set; }

        public ICollection<UsuarioCliente> ColUsuarioCadastro { get; set; }

        public ICollection<UsuarioCliente> ColUsuarioEdicao { get; set; }

        public ICollection<Pedido> ColPedido { get; set; }
    }
}
