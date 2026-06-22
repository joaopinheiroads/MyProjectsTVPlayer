using System.ComponentModel.DataAnnotations;

namespace Cardápio.Client.Dto
{
    public class UsuarioUpdateDTO
    {

        [Required]
        [StringLength(100, MinimumLength = 3)]
        public string Nome { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 3)]
        public string Email { get; set; }

        [Required]
        [StringLength(30, MinimumLength = 8)]
        public string Password { get; set; }

        [Required]
        public int? UsuarioTipoID { get; set; }

        [Required]
        public int? EmpresaID { get; set; }

        [Required]
        public bool Ativo { get; set; }
    }
}
