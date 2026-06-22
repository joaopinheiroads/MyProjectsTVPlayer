using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Cardápio.Client.Dto
{
    public class ImageFileAddDTO
    {
        [Column(TypeName = "varchar")]
        [MaxLength(100)]
        public string Nome { get; set; }

        [Column(TypeName = "varchar")]
        [MaxLength(50)]
        public string Arquivo { get; set; }
    }
}
