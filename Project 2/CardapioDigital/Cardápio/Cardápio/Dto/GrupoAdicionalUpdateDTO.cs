using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Cardápio.Dto
{
    public class GrupoAdicionalUpdateDTO
    {
        public int ID { get; set; }

        public string? Nome { get; set; }

        public int? EmpresaID { get; set; }

        public int? Minimo { get; set; }

        public int? Maximo { get; set; }

        public int? TipoId { get; set; }

        public ICollection<GrupoAdicionalItemAddDTO> ColGrupoAdicionalItem { get; set; }
    }
}
