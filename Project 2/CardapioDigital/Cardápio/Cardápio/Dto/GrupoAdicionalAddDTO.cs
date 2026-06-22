using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Cardápio.Dto
{
    public class GrupoAdicionalAddDTO
    {
        public string? Nome { get; set; }

        public int? EmpresaID { get; set; }

        public int? Minimo { get; set; }

        public int? Maximo { get; set; }

        public int? TipoId { get; set; }

        public ICollection<GrupoAdicionalItemAddDTO> ColGrupoAdicionalItem { get; set; }
    }

    public class GrupoAdicionalItemAddDTO
    {
        public int ID { get; set; }

        [Required]
        [MaxLength(100)]
        public string Nome { get; set; }

        [Required]
        [Column(TypeName = "decimal(19,2)")]
        public decimal? Preco { get; set; }

        [Required]
        public bool Ativo { get; set; }

        public string? ImageID { get; set; }

        public bool IsNovo { get; set; }
    }
}
