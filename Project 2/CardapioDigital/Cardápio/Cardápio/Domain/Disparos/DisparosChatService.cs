using System.Text;
using System.Text.RegularExpressions;




namespace Cardápio.Domain
{
    public class DisparosChatService
    {

        private readonly string TelefoneComercial = "1142101933";

        private readonly string _apiUrl = "https://v5.chatpro.com.br/[INSTANCIA_REMOVIDA]/api/v1/send_message";
        private readonly string _authToken = "[TOKEN_CHATPRO_REMOVIDO]"; // Mova isso para um local seguro

        public async Task<HttpResponseMessage> ChamarApiChatProAsync(string number, string message)
        {
            Console.WriteLine($"URL: {_apiUrl}");
            Console.WriteLine($"Token: {_authToken}");
            Console.WriteLine($"Número: {number}");
            Console.WriteLine($"Mensagem: {message}");

            using (var client = new HttpClient())
            {
                var payload = new { number = number, message = message };
                var json = System.Text.Json.JsonSerializer.Serialize(payload);

                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Post,
                    RequestUri = new Uri(_apiUrl),
                    Headers =
                    {
                        { "accept", "application/json" },
                        { "Authorization", _authToken }
                    },
                    Content = new StringContent(json, Encoding.UTF8, new System.Net.Http.Headers.MediaTypeHeaderValue("application/json"))
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

                try
                {
                    // Tenta extrair o nome do cliente da mensagem
                    string nomeCliente = Regex.Match(message, @"Olá\s\*(.*?)\*").Groups[1].Value;
                    string avisoErro = null;

                    if (message.Contains("homologar", StringComparison.OrdinalIgnoreCase))
                    {
                        avisoErro = $"Falha ao enviar mensagem de WhatsApp no *cadastro de demonstração* para o número {number}, cliente *{nomeCliente}*.";
                    }
                    else if (message.Contains("Demonstração", StringComparison.OrdinalIgnoreCase))
                    {
                        avisoErro = $"Falha ao enviar mensagem de WhatsApp no *alerta de vencimento de demonstração* para o número {number}, cliente *{nomeCliente}*.";
                    }

                    if (!string.IsNullOrEmpty(avisoErro))
                    {
                        await ChamarApiChatProAsync(TelefoneComercial, avisoErro);
                    }
                }
                catch (Exception innerEx)
                {
                    Console.WriteLine($"Falha ao notificar erro via WhatsApp: {innerEx.Message}");
                }
                return new EnvioMensagemResultado { Sucesso = false, Erro = ex.Message };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exceção inesperada: {ex.Message}");

                try
                {
                    // Tenta extrair o nome do cliente da mensagem
                    string nomeCliente = Regex.Match(message, @"Olá\s\*(.*?)\*").Groups[1].Value;
                    string avisoErro = null;

                    if (message.Contains("homologar", StringComparison.OrdinalIgnoreCase))
                    {
                        avisoErro = $"Falha ao enviar mensagem de WhatsApp no *cadastro de demonstração* para o número {number}, cliente *{nomeCliente}*.";
                    }
                    else if (message.Contains("Demonstração", StringComparison.OrdinalIgnoreCase))
                    {
                        avisoErro = $"Falha ao enviar mensagem de WhatsApp no *alerta de vencimento de demonstração* para o número {number}, cliente *{nomeCliente}*.";
                    }

                    if (!string.IsNullOrEmpty(avisoErro))
                    {
                        await ChamarApiChatProAsync(TelefoneComercial, avisoErro);
                    }
                }
                catch (Exception innerEx)
                {
                    Console.WriteLine($"Falha ao notificar erro via WhatsApp: {innerEx.Message}");
                }
                return new EnvioMensagemResultado { Sucesso = false, Erro = ex.Message };
            }
        }
    



}
}
