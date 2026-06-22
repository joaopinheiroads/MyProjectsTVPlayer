using System.ComponentModel.DataAnnotations;

namespace Cardápio.Client.Dto
{
    public class UsuarioAddDTO
    {

        [Required]
        [StringLength(100, MinimumLength = 3)]
        public string? Nome { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 3)]
        public string? Email { get; set; }

        [Required]
        [StringLength(30, MinimumLength = 8)]
        public string? Password { get; set; }

        public int? UsuarioTipoID { get; set; }

        public int? EmpresaID { get; set; }
    }

    public class NovoUsuarioSistemaDTO
    {
        [Required]
        [StringLength(100, MinimumLength = 3)]
        public string Nome { get; set; }
        [Required]
        [StringLength(100, MinimumLength = 3)]
        public string Email { get; set; }
        [Required]
        [StringLength(100, MinimumLength = 4)]
        public string Senha { get; set; }
        [StringLength(100, MinimumLength = 4)]
        public string ConfirmarSenha { get; set; }
        [Required]
        [StringLength(100, MinimumLength = 3)]
        public string EmpresaNome { get; set; }
        [Required]
        [StringLength(11, MinimumLength = 11)]
        public string Celular { get; set; }
        [Required]
        [StringLength(10, MinimumLength = 10)]
        public string Telefone { get; set; }
        [Required]
        [StringLength(8, MinimumLength = 8)]
        public string Cep { get; set; }

        [Required]
        [StringLength(2, MinimumLength = 2)]
        public string EstadoID { get; set; }
        [Required]
        [StringLength(20, MinimumLength = 2)]
        public int CidadeID { get; set; }
    }
}
