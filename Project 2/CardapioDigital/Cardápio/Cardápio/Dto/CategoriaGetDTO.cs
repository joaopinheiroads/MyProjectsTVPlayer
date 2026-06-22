using System.ComponentModel.DataAnnotations;

namespace Cardápio.Dto
{
    public class CategoriaGetDTO
    {
        public int ID { get; set; }

        [Required]
        [MaxLength(100)]
        public string Nome { get; set; }

        [Required]
        public string BackgroundColor { get; set; }

        [Required]
        public int? Ordem { get; set; }

        [Required]
        public DateTime DataCadastro { get; set; }
    }
}
