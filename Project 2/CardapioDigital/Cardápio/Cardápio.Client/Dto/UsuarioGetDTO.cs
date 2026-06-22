using System.ComponentModel.DataAnnotations;

namespace Cardápio.Client.Dto
{
    public class UsuarioGetDTO
    {
        public int ID { get; set; }

        [Required]
        [MaxLength(100)]
        public string Nome { get; set; }

        [Required]
        [MaxLength(100)]
        public string Email { get; set; }

        [Required]
        [MaxLength(30)]
        public string Password { get; set; }

        [Required]
        [MaxLength(50)]
        public string UsuarioTipo { get; set; }

        [Required]
        public int? UsuarioTipoID { get; set; }

        [Required]
        [MaxLength(100)]
        public string EmpresaNome { get; set; }

        public string GrupoNome { get; set; }

        [Required]
        public bool Ativo { get; set; }

        [Required]
        public int? EmpresaID { get; set; }

        public int? GrupoID { get; set; }
    }
}
