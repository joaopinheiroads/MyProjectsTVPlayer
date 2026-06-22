using System.ComponentModel.DataAnnotations;

namespace Cardápio.Dto
{
    public class EstadoGetDTO
    {
        [Required]
        [Key]
        [StringLength(2, MinimumLength = 2)]
        public string? EndEstadoID { get; set; }

        public string EstNome { get; set; }
    }
}
