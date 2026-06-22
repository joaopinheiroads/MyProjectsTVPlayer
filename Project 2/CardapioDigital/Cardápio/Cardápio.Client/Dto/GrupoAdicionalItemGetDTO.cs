using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Cardápio.Client.Dto
{
    public class GrupoAdicionalItemGetDTO
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

        [Required]
        public DateTime DataCadastro { get; set; }

        public ProdutoImagemGetDTO ColGrupoAdicionalItemImagem { get; set; }
    }
}
