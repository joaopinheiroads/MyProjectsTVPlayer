using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace Cardápio.Infra.Model
{
    public class Company
    {
        public int ID { get; set; }

        [Column(TypeName = "varchar")]
        [MaxLength(100)]
        public string? Nome { get; set; }

        [Column(TypeName = "varchar")]
        [MaxLength(100)]
        public string? QRCode { get; set; }

        [Column(TypeName = "varchar")]
        [MaxLength(100)]
        public string? RazaoSocial { get; set; }

        [Column(TypeName = "char")]
        [MaxLength(18)]
        public string? CNPJ { get; set; }

        [Column(TypeName = "char")]
        [MaxLength(15)]
        public string? Celular { get; set; }

        [Column(TypeName = "char")]
        [StringLength(14, MinimumLength = 10)]
        public string? Telefone { get; set; }

        [Column(TypeName = "char")]
        [StringLength(8, MinimumLength = 8)]
        public string? CEP { get; set; }

        [Column(TypeName = "char")]
        [StringLength(2, MinimumLength = 2)]
        public string? EstadoID { get; set; }

        [Required]
        public int? CidadeID { get; set; }

        [Required]
        public int? DiasDemo { get; set; }

        [Required]
        public int? UsuarioIDCadastro { get; set; }

        [ForeignKey(nameof(UsuarioIDCadastro))]
        public User UsuarioCadastro { get; set; }

        public int? UsuarioIDEdicao { get; set; }

        [ForeignKey(nameof(UsuarioIDEdicao))]
        public User UsuarioEdicao { get; set; }

        public bool AtenderWhatsapp { get; set; }

        [Required]
        public int? EmpresaTipoID { get; set; }

        [ForeignKey(nameof(EmpresaTipoID))]
        public UsuarioStatus EmpresaTipo { get; set; }

        [Required]
        public int? GrupoID { get; set; }

        [ForeignKey(nameof(GrupoID))]
        public Group Grupo { get; set; }

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

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [DefaultValue(true)]
        public bool Excluido { get; set; }

        public ICollection<Banner> ColBanner { get; set; }

        public ICollection<QrCodeLayout> ColQrCodeLayout { get; set; }

        public ICollection<Logo> ColLogo { get; set; }

        public ICollection<UsuarioEmpresa> ColUsuarioEmpresa { get; set; }

        public ICollection<User> ColUsuario { get; set; }

        public ICollection<Product> ColProduto { get; set; }

        public ICollection<Category> ColCategoria { get; set; }

        public ICollection<GrupoAdicionalItem> ColGrupoAdicionalItem { get; set; }

        public ICollection<GrupoAdicional> ColGrupoAdicional { get; set; }

        public ICollection<Pedido> ColPedido { get; set; }
    }
}
