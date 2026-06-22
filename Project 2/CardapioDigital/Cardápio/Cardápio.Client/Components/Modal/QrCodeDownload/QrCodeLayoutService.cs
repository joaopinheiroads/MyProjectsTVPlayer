using Cardápio.Client.Dto;
using System.Net.Http.Headers;
using System.Net.Http;
using Cardápio.Client.Infra.Provider;
using System.Net.Http.Json;

namespace Cardápio.Client.Components.Modal.QrCodeDownload
{
    public class QrCodeLayoutService
    {
        private HttpClient _httpClient;
        private CustomAuthenticationStateProvider _provider;

        public QrCodeLayoutService(HttpClient httpClient, CustomAuthenticationStateProvider provider)
        {
            _httpClient = httpClient;
            _provider = provider;
        }

        public async Task<List<QrCodeLayoutGetDTO>> GetData(int companyID)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", await _provider.GetTokenAsync());

                List<QrCodeLayoutGetDTO> result = await _httpClient.GetFromJsonAsync<List<QrCodeLayoutGetDTO>>("/api/qrcode/data/" + companyID);

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return null;
        }

        public async Task OnCreateQrCodeLayoutConfirm(int companyID, QrCodeLayoutAddDTO qrCodeLayout)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", await _provider.GetTokenAsync());
                string requestUrl = $"/api/qrcode/data/{companyID}";

                HttpResponseMessage response = await _httpClient.PostAsJsonAsync(requestUrl, qrCodeLayout);

                if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    string errorContent = await response.Content.ReadAsStringAsync();
                    throw new Exception(errorContent);
                }

                response.EnsureSuccessStatusCode();
            }
            catch (Exception err)
            {
                throw err;
            }
        }
    }
}
