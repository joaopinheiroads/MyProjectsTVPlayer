using System.ComponentModel.DataAnnotations;

namespace Cardápio.Dto
{
    public class UsuarioTipoGetDTO
    {
        public int ID { get; set; }

        [Required]
        [MaxLength(50)]
        public string Nome { get; set; }
    }
}
