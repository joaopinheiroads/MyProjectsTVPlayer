using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Cardápio.Infra.Model
{
    public class State
    {
        [Key]
        [Required]
        [Column(TypeName = "char")]
        [StringLength(2, MinimumLength = 2)]
        public string EstID { get; set; }

        [Required]
        [Column(TypeName = "varchar")]
        [MaxLength(72)]
        public string EstNome { get; set; }
    }
}
