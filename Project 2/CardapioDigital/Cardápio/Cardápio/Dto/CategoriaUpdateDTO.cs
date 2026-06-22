using System.ComponentModel.DataAnnotations;

namespace Cardápio.Dto
{
    public class CategoriaUpdateDTO
    {
        public int ID { get; set; }

        [MaxLength(100)]
        public string Nome { get; set; }

        public string? BackgroundColor { get; set; }

        public int? Ordem { get; set; }
    }
}
