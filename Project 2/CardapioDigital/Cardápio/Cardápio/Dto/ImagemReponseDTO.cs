using System.ComponentModel.DataAnnotations;

namespace Cardápio.Dto
{
    public class ImagemReponseDTO
    {
        public ImagemReponseDTO(bool success, string result)
        {
            this.Success = success;
            this.Result = result;
        }

        [Required]
        public bool Success { get; set; }

        [Required]
        public string Result { get; set; }
    }
}
