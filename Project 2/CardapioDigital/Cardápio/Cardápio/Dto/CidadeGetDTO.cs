using System.ComponentModel.DataAnnotations;

namespace Cardápio.Dto
{
    public class CidadeGetDTO
    {
        [Key]
        public int? CidID { get; set; }

        [Required]
        [MaxLength(60)]
        public string CidNome { get; set; }
    }
}
