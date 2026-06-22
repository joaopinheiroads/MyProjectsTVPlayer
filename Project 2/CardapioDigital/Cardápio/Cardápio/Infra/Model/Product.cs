using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace Cardápio.Infra.Model
{
    public class Product
    {
        public int ID { get; set; }

        [Required]
        [Column(TypeName = "varchar")]
        [MaxLength(100)]
        public string Nome { get; set; } = string.Empty;

        [Column(TypeName = "varchar")]
        [MaxLength(400)]
        public string? Descricao { get; set; }

        [Required]
        [Column(TypeName = "decimal(19,2)")]
        public decimal? Preco { get; set; }

        [Required]
        public bool Promocao { get; set; }

        [Required]
        public int QTDPessoa { get; set; }

        [Column(TypeName = "decimal(19,2)")]
        public decimal? PrecoPromocional { get; set; }

        public DateTime? FimPromocao { get; set; }

        [Required]
        public bool Destaque { get; set; }

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
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [DefaultValue(true)]
        public bool Ativo { get; set; }

        [Required]
        public bool Excluido { get; set; }

        [Required]
        public int? CategoriaID { get; set; }

        [ForeignKey(nameof(CategoriaID))]
        public Category? Categoria { get; set; }

        [Required]
        public int? EmpresaID { get; set; }

        [ForeignKey(nameof(EmpresaID))]
        public Company? Empresa { get; set; }

        public int? ProdutoThumbnailID { get; set; }

        [ForeignKey(nameof(ProdutoThumbnailID))]
        public ThumbnailProduct? ProdutoThumbnail { get; set; }

        public ICollection<ImageProduct> ColProdutoImagem { get; set; } = new List<ImageProduct>();

        public ICollection<ProdutoGrupoAdicional> ProdutoGruposAdicional { get; set; } = new List<ProdutoGrupoAdicional>();

        public ICollection<ProdutoHorario> ProdutoHorarios { get; set; } = new List<ProdutoHorario>();
        
        public ICollection<ProdutoPromocao> ProdutoPromocoes { get; set; } = new List<ProdutoPromocao>();
        
        public ICollection<ProdutoPromocaoHorario> ProdutoPromocaoHorarios { get; set; } = new List<ProdutoPromocaoHorario>();
    }
}
