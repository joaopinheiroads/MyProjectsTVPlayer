using System.Text.Json.Serialization;

namespace Cardápio.Client.Dto
{
    public class ErrorResponse
    {
        [JsonPropertyName("messageError")]
        public string MessageError { get; set; }

        [JsonPropertyName("codeError")]
        public int CodeError { get; set; }
    }
}
