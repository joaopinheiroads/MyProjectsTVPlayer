using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Cardápio.Client.Dto
{
    public class GrupoAdicionalAddDTO
    {
        [Required]
        [StringLength(50, MinimumLength = 3)]
        public string? Nome { get; set; }

        public int? EmpresaID { get; set; }

        public int? Minimo { get; set; }

        public int? Maximo { get; set; }

        public int? TipoId { get; set; }

        public List<GrupoAdicionalItemAddDTO> ColGrupoAdicionalItem { get; set; }
    }

    public class GrupoAdicionalItemAddDTO
    {
        public int ID { get; set; }

        [Required]
        [StringLength(50, MinimumLength = 3)]
        public string Nome { get; set; }

        [Required]
        [Column(TypeName = "decimal(19,2)")]
        public decimal? Preco { get; set; }

        [Required]
        public bool Ativo { get; set; }

        public bool IsNovo { get; set; }

        public string? ImageID { get; set; }
    }
}
