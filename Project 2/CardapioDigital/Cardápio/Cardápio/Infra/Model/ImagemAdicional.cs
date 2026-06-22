using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace Cardápio.Infra.Model
{
    public class GrupoAdicionalItemImagem
    {
        public int ID { get; set; }

        [Required]
        [Column(TypeName = "varchar")]
        [MaxLength(100)]
        public string Nome { get; set; }

        [Required]
        [Column(TypeName = "varchar")]
        [MaxLength(50)]
        public string Arquivo { get; set; }

        [Required]
        public int Tamanho { get; set; }

        [Required]
        public int Largura { get; set; }

        [Required]
        public int Altura { get; set; }

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
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [DefaultValue(true)]
        public bool Ativo { get; set; }

        [Required]
        public int? GrupoAdicionalItemID { get; set; }

        [ForeignKey(nameof(GrupoAdicionalItemID))]
        public GrupoAdicionalItem GrupoAdicionalItem { get; set; }
    }
}
