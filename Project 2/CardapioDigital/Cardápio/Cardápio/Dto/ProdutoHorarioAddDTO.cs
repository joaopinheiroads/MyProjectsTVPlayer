using System.ComponentModel.DataAnnotations;

namespace Cardápio.Dto
{
    public class ProdutoHorarioAddDTO
    {
        [Required]
        public int ProdutoID { get; set; }
        
        [Required]
        [MaxLength(5)]
        public string HoraInicio { get; set; } = "";
        
        [Required]
        [MaxLength(5)]
        public string HoraFim { get; set; } = "";
        
        [Required]
        [MaxLength(20)]
        public string DiaSemana { get; set; } = "";
        
        public bool Ativo { get; set; } = true;
    }
}
