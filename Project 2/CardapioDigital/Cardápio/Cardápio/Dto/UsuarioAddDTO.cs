using System.ComponentModel.DataAnnotations;

namespace Cardápio.Dto
{
    public class UsuarioAddDTO
    {
        [Required]
        [MaxLength(100)]
        public string Nome { get; set; }

        [Required]
        [MaxLength(100)]
        public string Email { get; set; }

        [Required]
        [MinLength(6)]
        [MaxLength(30)]
        public string Password { get; set; }

        [Required]
        public int? UsuarioTipoID { get; set; }

        [Required]
        public int? EmpresaID { get; set; }
    }

    public class NovoUsuarioSistemaDTO
    {
        public string Nome { get; set; }
        public string Email { get; set; }
        public string EmpresaNome { get; set; }
        public string Senha { get; set; }
        public string Celular { get; set; }
        public string Telefone { get; set; }
        public string Cep { get; set; }

        public string EstadoID { get; set; }
        public int CidadeID { get; set; }
    }
}
