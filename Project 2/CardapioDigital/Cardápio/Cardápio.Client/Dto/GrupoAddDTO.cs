using System.ComponentModel.DataAnnotations;

namespace Cardápio.Client.Dto
{
    public class GrupoAddDTO
    {
        [Required(ErrorMessage = "O Nome é obrigatório.")]
        [MaxLength(100)]
        public string Nome { get; set; }

        public string NomeEmpresa { get; set; }

        public int EmpresaTipoID { get; set; }

        public string NomeUsuarioAdmin { get; set; }

        public string UsuarioLogin { get; set; }

        public string UsuarioSenha { get; set; }

        public string UsuarioEmail { get; set; }

        public bool Ativo { get; set; }
    }
}
