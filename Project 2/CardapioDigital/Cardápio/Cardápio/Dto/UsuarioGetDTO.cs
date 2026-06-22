
using System.ComponentModel.DataAnnotations;

namespace Cardápio.Dto
{
    public class UsuarioGetDTO
    {
        public int ID { get; set; }

        [MaxLength(100)]
        public string? Nome { get; set; }

        [MaxLength(100)]
        public string? Email { get; set; }

        [MaxLength(30)]
        public string? Password { get; set; }

        [MaxLength(50)]
        public string? UsuarioTipo { get; set; }

        public int? UsuarioTipoID { get; set; }
        public int? UsuarioStatusID { get; set; }

        [MaxLength(100)]
        public string? EmpresaNome { get; set; }

        public string? GrupoNome { get; set; }

        [Required]
        public int? EmpresaID { get; set; }

        public int? GrupoID { get; set; }

        public bool Ativo { get; set; }
    }
}
