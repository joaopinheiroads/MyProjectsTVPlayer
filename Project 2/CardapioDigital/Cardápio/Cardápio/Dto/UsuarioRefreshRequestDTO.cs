using System.ComponentModel.DataAnnotations;

namespace Cardápio.Dto
{
    public class UsuarioRefreshRequestDTO
    {
        [Required]
        public string Token { get; set; }

        [Required]
        public string RefreshToken { get; set; }
    }
}
