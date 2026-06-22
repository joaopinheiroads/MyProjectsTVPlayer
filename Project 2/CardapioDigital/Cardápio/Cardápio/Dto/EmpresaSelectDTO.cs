using System.ComponentModel.DataAnnotations;

namespace Cardápio.Dto
{
    public class EmpresaSelectDTO
    {
        public int ID { get; set; }

        [Required]
        [MaxLength(100)]
        public string Nome { get; set; }
    }
}
