using Cardápio.Client.Dto;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Net.Http.Json;
using Cardápio.Client.Infra.Provider;

namespace Cardápio.Client.Pages.Mesas
{
    public class MesasService
    {
        private HttpClient _httpClient;
        private CustomAuthenticationStateProvider _provider;

        public MesasService(HttpClient httpClient, CustomAuthenticationStateProvider provider)
        {
            _httpClient = httpClient;
            _provider = provider;
        }

        public async Task<List<MesaGetDTO>> GetData(int companyID)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", await _provider.GetTokenAsync());

                List<MesaGetDTO> result = await _httpClient.GetFromJsonAsync<List<MesaGetDTO>>("/api/mesa/data/" + companyID);

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return null;
        }

        public async Task OnCreateMesaConfirm(int companyID)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", await _provider.GetTokenAsync());
                string requestUrl = $"/api/mesa/data/{companyID}";

                HttpResponseMessage response = await _httpClient.PostAsync(requestUrl, null);

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

        public async Task OnDeleteMesaConfirm(int mesaID, int empresaID)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", await _provider.GetTokenAsync());
                string requestUrl = $"/api/mesa/data/{mesaID}/{empresaID}";

                HttpResponseMessage response = await _httpClient.DeleteAsync(requestUrl);

                if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    string errorContent = await response.Content.ReadAsStringAsync();
                    throw new Exception(errorContent);
                }

                response.EnsureSuccessStatusCode();
            } catch (Exception err)
            {
                throw err;
            }
        }

        public async Task OnEditMesaConfirm(int empresaID, MesaUpdateDTO mesaUpdateDTO)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", await _provider.GetTokenAsync());
                string requestUrl = $"/api/mesa/data/{empresaID}";

                HttpResponseMessage response = await _httpClient.PutAsJsonAsync(requestUrl, mesaUpdateDTO);

                if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    string errorContent = await response.Content.ReadAsStringAsync();
                    throw new Exception(errorContent);
                }

                response.EnsureSuccessStatusCode();
            } catch (Exception err)
            {
                throw err;
            }
        }
    }
}
