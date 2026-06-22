using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Cardápio.Infra.Model
{
    public class SolicitacaoDemonstracao
    {
        [Key]
        public Guid ID { get; set; }

        public bool Ativo { get; set; }

        public int? GrupoID { get; set; }

        [ForeignKey(nameof(GrupoID))]
        public Group Grupo { get; set; }
    }
}
