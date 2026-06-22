using Cardápio.Client.Dto;
using System.Net.Http.Json;

namespace Cardápio.Client.Pages.AuthClient.OptionsForm
{
    public class SmsCodeSenderService : ICodeSenderService
    {
        public readonly HttpClient _httpClient;

        public SmsCodeSenderService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task SendCodeAsync(string destination)
        {
            try
            {
                CodigoVerificacaoToAddDTO account = new()
                {
                    Phone = destination,
                };

                var response = await _httpClient.PostAsJsonAsync("/api/code/send-code", account);
            }
            catch (Exception ex)
            {

            }
        }

        public async Task<HttpResponseMessage> VerifyCodeAsync(string destination, string code)
        {
            try
            {
                CodigoVerificacaoValueAddDTO codeDTO = new()
                {
                    Phone = destination,
                    Code = code,
                };

                HttpResponseMessage response = await _httpClient.PostAsJsonAsync("/api/code/verify-code", codeDTO);

                return response;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
