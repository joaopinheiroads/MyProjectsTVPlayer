using System.ComponentModel.DataAnnotations;

namespace Cardápio.Client.Dto
{
    public class CategoriaAddDTO
    {
        [Required]
        [StringLength(100, MinimumLength = 3)]
        public string Nome { get; set; }

        [Required]
        [StringLength(20, MinimumLength = 5)]
        public string BackgroundColor { get; set; }
    }
}
