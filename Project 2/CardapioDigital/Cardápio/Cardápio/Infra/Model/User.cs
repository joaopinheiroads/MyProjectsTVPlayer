using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace Cardápio.Infra.Model
{
    public class User
    {
        public int ID { get; set; }

        [Column(TypeName = "varchar")]
        [MaxLength(100)]
        public string? Nome { get; set; }

        [Column(TypeName = "varchar")]
        [MaxLength(100)]
        [EmailAddress]
        public string? Email { get; set; }

        [Column(TypeName = "varchar")]
        [MinLength(6)]
        [MaxLength(30)]
        public string? Password { get; set; }

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
        public int? UsuarioStatusID { get; set; }

        [ForeignKey(nameof(UsuarioStatusID))]
        public UsuarioStatus UsuarioStatus { get; set; }

        [Required]
        public int? GrupoID { get; set; }

        [ForeignKey(nameof(GrupoID))]
        public Group Grupo { get; set; }

        [Required]
        public int? EmpresaID { get; set; }

        [ForeignKey(nameof(EmpresaID))]
        public Company Empresa { get; set; }

        [Required]
        public int? UsuarioIDCadastro { get; set; }

        [ForeignKey(nameof(UsuarioIDCadastro))]
        public User UsuarioCadastro { get; set; }

        public int? UsuarioIDEdicao { get; set; }

        [ForeignKey(nameof(UsuarioIDEdicao))]
        public User UsuarioEdicao { get; set; }

        public bool Excluido { get; set; }

        public ICollection<UsuarioEmpresa> ColUsuarioEmpresa { get; set; }

        public ICollection<Product> ColProdutoCadastro { get; set; }

        public ICollection<Product> ColProdutoEdicao { get; set; }

        public ICollection<Category> ColCategoriaCadastro { get; set; }

        public ICollection<Category> ColCategoriaEdicao { get; set; }

        public ICollection<UserGroup> ColGrupoUsuario { get; set; }

        public ICollection<Group> ColGrupoCadastro { get; set; }

        public ICollection<Group> ColGrupoEdicao { get; set; }

        public ICollection<UserGroup> ColGrupoUsuarioCadastro { get; set; }

        public ICollection<UserGroup> ColGrupoUsuarioEdicao { get; set; }

        public ICollection<Company> ColEmpresaCadastro { get; set; }

        public ICollection<Company> ColEmpresaEdicao { get; set; }

        public ICollection<User> ColUsuarioCadastro { get; set; }

        public ICollection<User> ColUsuarioEdicao { get; set; }

        public ICollection<GrupoAdicionalItem> ColGrupoAdicionalItemCadastro { get; set; }

        public ICollection<GrupoAdicionalItem> ColGrupoAdicionalItemEdicao { get; set; }

        public ICollection<GrupoAdicional> ColGrupoAdicionalCadastro { get; set; }

        public ICollection<GrupoAdicional> ColGrupoAdicionalEdicao { get; set; }
    }
}
