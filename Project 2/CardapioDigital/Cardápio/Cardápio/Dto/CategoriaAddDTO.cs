using System.ComponentModel.DataAnnotations;

namespace Cardápio.Dto
{
    public class CategoriaAddDTO
    {
        [Required(ErrorMessage = "O Nome é obrigatório.")]
        [MaxLength(100)]
        public string Nome { get; set; }

        [Required(ErrorMessage = "A Cor é obrigatória")]
        public string BackgroundColor { get; set; }
    }
} 
