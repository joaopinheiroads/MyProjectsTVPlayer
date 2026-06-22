using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Cardápio.Infra.Model
{
    public class TipoGrupoAdicional
    {
        public int ID { get; set; }

        [Required]
        [Column(TypeName = "varchar")]
        [MaxLength(100)]
        public string Tipo { get; set; }

        public ICollection<GrupoAdicional> ColGrupoAdicional { get; set; }
    }
}
