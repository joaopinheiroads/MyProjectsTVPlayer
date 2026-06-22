using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace Cardápio.Infra.Model
{
    public class UsuarioStatus
    {
        public int ID { get; set; }

        [Required]
        [Column(TypeName = "varchar")]
        [MaxLength(50)]
        public string Nome { get; set; }

        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [DefaultValue(true)]
        public bool Ativo { get; set; }

        public ICollection<User> ColUsuario { get; set; }
    }
}
