using System.ComponentModel.DataAnnotations;

namespace Cardápio.Dto
{
    public class EnderecoDto
    {
        [Key]
        public int CidID { get; set; }

        public string EndNome { get; set; }
        public string EndEstadoID { get; set; }
        public string CidNome { get; set; }
        public string BaiNome { get; set; }
        public string EndComplemento { get; set; }
    }
}
