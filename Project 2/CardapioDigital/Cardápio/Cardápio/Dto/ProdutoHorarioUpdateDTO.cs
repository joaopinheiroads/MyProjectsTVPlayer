using System.ComponentModel.DataAnnotations;

namespace Cardápio.Dto
{
    public class ProdutoHorarioUpdateDTO
    {
        public int ID { get; set; }
        
        [Required]
        [MaxLength(5)]
        public string HoraInicio { get; set; } = "";
        
        [Required]
        [MaxLength(5)]
        public string HoraFim { get; set; } = "";
        
        [Required]
        [MaxLength(20)]
        public string DiaSemana { get; set; } = "";
        
        public bool Ativo { get; set; }
    }
}
