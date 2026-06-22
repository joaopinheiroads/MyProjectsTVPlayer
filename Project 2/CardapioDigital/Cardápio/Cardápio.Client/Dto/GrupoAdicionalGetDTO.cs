using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Cardápio.Client.Dto
{
    public class GrupoAdicionalGetDTO
    {
        public int ID { get; set; }

        public string Nome { get; set; }

        public bool Ativo { get; set; }

        public int? EmpresaID { get; set; }

        public int Minimo { get; set; }

        public int Maximo { get; set; }

        public string TipoNome { get; set; }

        public int TipoID { get; set; }

        [Required]
        [Column(TypeName = "decimal(19,2)")]
        public decimal? Preco { get; set; }

        public ICollection<GrupoAdicionalItemGetDTO> ColGrupoAdicionalItem { get; set; }

        public ICollection<ProdutoGetDTO>? ColProducts { get; set; }

        public bool IsCollapsed { get; set; }

        public int? Posicao { get; set; }

    }
}
