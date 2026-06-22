using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Cardápio.Client.Dto
{
    public class ProdutoImagemAddDTO
    {
        [Required]
        [Column(TypeName = "varchar")]
        [MaxLength(100)]
        public string Nome { get; set; }

        [Required]
        [Column(TypeName = "varchar")]
        [MaxLength(50)]
        public string Arquivo { get; set; }
    }
}
