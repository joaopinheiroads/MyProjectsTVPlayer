using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Cardápio.Dto
{
    public class ProdutoThumbnailAddDTO
    {
        [Column(TypeName = "varchar")]
        [MaxLength(100)]
        public string Nome { get; set; }

        [Column(TypeName = "varchar")]
        [MaxLength(50)]
        public string Arquivo { get; set; }
    }
}
