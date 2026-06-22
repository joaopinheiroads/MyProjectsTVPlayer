using System.Text.Json.Serialization;

namespace Cardápio.Dto
{
    public class AuthenticationDto
    {
        [JsonPropertyName("user")]
        public UserAuthenticatedDto _userAuthenticatedDto { get; set; }

        [JsonPropertyName("_token")]
        public string _Token { get; set; }

        public AuthenticationDto(UserAuthenticatedDto authenticationDto, string Token)
        {
            _userAuthenticatedDto = authenticationDto;
            _Token = Token;
        }
    }

    public class UserAuthenticatedDto
    {
        [JsonPropertyName("email")]
        public string _Email { get; set; }

        public UserAuthenticatedDto(string email)
        {
            _Email = email;
        }
    }
}
