using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Cardápio.Infra.Model
{
    public class City
    {
        [Key]
        public int CidID { get; set; }

        [Required]
        [Column(TypeName = "varchar")]
        [MaxLength(60)]
        public string CidNome { get; set; }

        [Required]
        [Column(TypeName = "char")]
        [StringLength(2, MinimumLength = 2)]
        public string CidEstadoID { get; set; }

        [Required]
        [Column(TypeName = "char")]
        [StringLength(9, MinimumLength = 9)]
        public string CidCep { get; set; }
    }
}
