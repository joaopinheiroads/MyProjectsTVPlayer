using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace Cardápio.Infra.Model
{
    public class Group
    {
        public int ID { get; set; }

        [Column(TypeName = "varchar")]
        [MaxLength(100)]
        public string Nome { get; set; }

        public int? UsuarioIDCadastro { get; set; }

        [ForeignKey(nameof(UsuarioIDCadastro))]
        public User UsuarioCadastro { get; set; }

        public int? UsuarioIDEdicao { get; set; }

        [ForeignKey(nameof(UsuarioIDEdicao))]
        public User UsuarioEdicao { get; set; }

        [Column(TypeName = "datetime2(0)")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime DataCadastro { get; set; }

        [Column(TypeName = "datetime2(0)")]
        public DateTime? DataEdicao { get; set; }

        [Required]
        public int? GrupoTipoID { get; set; }

        [ForeignKey(nameof(GrupoTipoID))]
        public UsuarioStatus GrupoTipo { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [DefaultValue(true)]
        public bool Excluido { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [DefaultValue(true)]
        public bool Ativo { get; set; }

        public ICollection<UserGroup> ColGrupoUsuario { get; set; }

        public ICollection<User> ColUsers { get; set; }

        public ICollection<SolicitacaoDemonstracao> ColSolicitacao { get; set; }

        public ICollection<Company> ColEmpresa { get; set; }
    }
}
