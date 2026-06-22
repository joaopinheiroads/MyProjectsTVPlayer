using System.ComponentModel.DataAnnotations;

namespace Cardápio.Dto
{
    public class UsuarioUpdateDTO
    {

        [Required]
        [MaxLength(100)]
        public string Nome { get; set; }

        [Required]
        [MaxLength(100)]
        public string Email { get; set; }

        //[Required]
        //[MinLength(6)]
        //[MaxLength(30)]
        public string Password { get; set; }

        [Required]
        public int? UsuarioTipoID { get; set; }

        [Required]
        public int? EmpresaID { get; set; }


        [Required]
        public bool Ativo { get; set; }
    }
}
