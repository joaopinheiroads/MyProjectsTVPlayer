using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace Cardápio.Infra.Model
{
    public class Mesa
    {
        public int ID { get; set; }

        public string NomeMesa { get; set; }

        public string QRCode { get; set; }
        public int MesaStatusID { get; set; }
        [ForeignKey(nameof(MesaStatusID))]
        public MesaStatus MesaStatus { get; set; }

        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [DefaultValue(true)]
        public bool Ativo { get; set; }

        [Required]
        public int? UsuarioIDCadastro { get; set; }

        [ForeignKey(nameof(UsuarioIDCadastro))]
        public User UsuarioCadastro { get; set; }

        public int? UsuarioIDEdicao { get; set; }

        [ForeignKey(nameof(UsuarioIDEdicao))]
        public User UsuarioEdicao { get; set; }

        [Column(TypeName = "datetime2(0)")]
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime DataCadastro { get; set; }

        [Column(TypeName = "datetime2(0)")]
        public DateTime? DataEdicao { get; set; }

        [Required]
        public int? EmpresaID { get; set; }

        [ForeignKey(nameof(EmpresaID))]
        public Company Empresa { get; set; }

        public ICollection<Pedido> ColPedido { get; set; }
    }
}
