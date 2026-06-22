using Cardápio.Client.Pages;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace Cardápio.Infra.Model
{
    public class UserGroup
    {
        public int ID { get; set; }

        [Required]
        public int? GrupoID { get; set; }

        [ForeignKey(nameof(GrupoID))]
        public Group Grupo { get; set; }

        [Required]
        public int? UsuarioID { get; set; }

        public bool IsAdmin { get; set; }

        [ForeignKey(nameof(UsuarioID))]
        public User Usuario { get; set; }

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
    }
}
