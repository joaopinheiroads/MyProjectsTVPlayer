using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;

namespace Cardápio.Infra.Model
{
    [Table("Promocao")]
    public class Promocao
    {
        [Key]
        public int ID { get; set; }

        [Required]
        [Column(TypeName = "nvarchar")]
        [MaxLength(100)]
        public string Nome { get; set; } = string.Empty;

        [Column(TypeName = "nvarchar")]
        [MaxLength(255)]
        public string? Descricao { get; set; }

        [Required]
        public DateTime DataInicio { get; set; }

        [Required]
        public DateTime DataFim { get; set; }

        [Required]
        [DefaultValue(true)]
        public bool Ativo { get; set; } = true;

        [Required]
        public int? UsuarioIDCadastro { get; set; }

        [ForeignKey(nameof(UsuarioIDCadastro))]
        public User? UsuarioCadastro { get; set; }

        public int? UsuarioIDEdicao { get; set; }

        [ForeignKey(nameof(UsuarioIDEdicao))]
        public User? UsuarioEdicao { get; set; }

        [Column(TypeName = "datetime2(0)")]
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime DataCadastro { get; set; }

        [Column(TypeName = "datetime2(0)")]
        public DateTime? DataEdicao { get; set; }

        [Required]
        public int? EmpresaID { get; set; }

        [ForeignKey(nameof(EmpresaID))]
        public Company? Empresa { get; set; }

        // Navigation Properties
        public ICollection<PromocaoHorario> PromocaoHorarios { get; set; } = new List<PromocaoHorario>();
        
        public ICollection<ProdutoPromocao> ProdutoPromocoes { get; set; } = new List<ProdutoPromocao>();
    }
}
