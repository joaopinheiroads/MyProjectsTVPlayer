using System.ComponentModel.DataAnnotations;

namespace Cardápio.Client.Dto
{
    public class CidadeGetDTO
    {
        public int CidID { get; set; }

        [Required]
        [MaxLength(60)]
        public string CidNome { get; set; } = "";
    }
}
