using System;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TVPlayerSite.API.Disparos
{
    public class DisparosChat
    {

        private readonly string TelefoneComercial = "11959653000";

        private readonly string _apiUrl = "https://v5.chatpro.com.br/[INSTANCIA_REMOVIDA]/api/v1/send_message";
        private readonly string _authToken = "[TOKEN_CHATPRO_REMOVIDO]"; // deve vir de configuração/variável de ambiente

        public async Task<HttpResponseMessage> ChamarApiChatProAsync(string number, string message)
        {
            Console.WriteLine($"URL: {_apiUrl}");
            Console.WriteLine($"Token: {_authToken}");
            Console.WriteLine($"Número: {number}");
            Console.WriteLine($"Mensagem: {message}");

            using (var client = new HttpClient())
            {
                var payload = new { number = number, message = message };
                var json = Newtonsoft.Json.JsonConvert.SerializeObject(payload);

                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Post,
                    RequestUri = new Uri(_apiUrl),
                    Headers =
                    {
                        { "accept", "application/json" },
                        { "Authorization", _authToken }
                    },
                    Content = new StringContent(json, Encoding.UTF8, "application/json")
                };
                return await client.SendAsync(request);
            }
        }

        // Novo resultado para status do envio
        public class EnvioMensagemResultado
        {
            public bool Sucesso { get; set; }
            public string Erro { get; set; }
        }

        // Altere o método para retornar status
        public async Task<EnvioMensagemResultado> EnviarMensagemAsync(string number, string message)
        {
            if (string.IsNullOrEmpty(number))
            {
                return new EnvioMensagemResultado { Sucesso = false, Erro = "Número de telefone inválido. Certifique-se de incluir o código do país." };
            }

            try
            {
                using (var response = await ChamarApiChatProAsync(number, message))
                {
                    response.EnsureSuccessStatusCode();
                    var body = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Mensagem enviada com sucesso: {body}");
                    return new EnvioMensagemResultado { Sucesso = true };
                }
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Erro ao enviar mensagem: {ex.Message}");

                
                return new EnvioMensagemResultado { Sucesso = false, Erro = ex.Message };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exceção inesperada: {ex.Message}");

              
                return new EnvioMensagemResultado { Sucesso = false, Erro = ex.Message };
            }
        }
    }
}
