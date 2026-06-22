using System.Text.Json.Serialization;

namespace Cardápio.Client.Dto
{
    public class AuthenticationDto
    {
        [JsonPropertyName("user")]
        public UserAuthenticatedDto User { get; set; }

        [JsonPropertyName("_token")]
        public string _Token { get; set; }
    }

    public class UserAuthenticatedDto
    {
        [JsonPropertyName("email")]
        public string Email { get; set; }
    }
}
