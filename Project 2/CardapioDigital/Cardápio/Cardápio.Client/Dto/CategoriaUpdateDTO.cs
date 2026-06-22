using System.ComponentModel.DataAnnotations;

namespace Cardápio.Client.Dto
{
    public class CategoriaUpdateDTO
    {
        [Required]
        public int ID { get; set; }

        [Required]
        [StringLength(70, MinimumLength = 3)]
        public string Nome { get; set; }

        [Required]
        [StringLength(70, MinimumLength = 3)]
        public string? BackgroundColor { get; set; }

        [Required]
        public int? Ordem { get; set; }
    }
}
