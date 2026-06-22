using System.ComponentModel.DataAnnotations;

namespace Cardápio.Client.Dto
{
    public class GrupoGetDTO
    {
        public int ID { get; set; }

        [Required]
        [MaxLength(100)]
        public string Nome { get; set; }

        public bool Ativo { get; set; }

        public bool Excluido { get; set; }

        public int GrupoTipoID { get; set; }

        public IEnumerable<EmpresaGetDTO> Empresas { get; set; }
    }
}
