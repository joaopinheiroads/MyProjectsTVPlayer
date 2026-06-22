using System.ComponentModel.DataAnnotations;

namespace Cardápio.Client.Dto
{
    public class GrupoUpdateDTO
    {
        [Required(ErrorMessage = "O Nome é obrigatório.")]
        [MaxLength(100)]
        public string Nome { get; set; }

        public bool Ativo { get; set; }

        public int GrupoTipoID { get; set; }
    }
}
