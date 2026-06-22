using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Cardápio.Infra.Model
{
    [Table("ProdutoPromocao")]
    public class ProdutoPromocao
    {
        [Key]
        public int ID { get; set; }

        [Required]
        public int ProdutoID { get; set; }

        [Required]
        public int PromocaoID { get; set; }

        [Required]
        [Column(TypeName = "decimal(19,2)")]
        public decimal PrecoPromocional { get; set; }

        public bool Ativo { get; set; } = true;

        public DateTime DataCadastro { get; set; } = DateTime.Now;

        public DateTime? DataEdicao { get; set; }

        public int UsuarioIDCadastro { get; set; }

        public int? UsuarioIDEdicao { get; set; }

        // Navigation Properties
        [ForeignKey("ProdutoID")]
        public virtual Product? Produto { get; set; }

        [ForeignKey("PromocaoID")]
        public virtual Promocao? Promocao { get; set; }

        [ForeignKey("UsuarioIDCadastro")]
        public virtual User? UsuarioCadastro { get; set; }

        [ForeignKey("UsuarioIDEdicao")]
        public virtual User? UsuarioEdicao { get; set; }
    }
}
