using System.ComponentModel.DataAnnotations;

namespace Cardápio.Client.Dto
{
    public class EstadoGetDTO
    {
        [Required]
        [StringLength(2, MinimumLength = 2)]
        public string EndEstadoID { get; set; } = "";

        public string EstNome { get; set; } = "";
    }
}
