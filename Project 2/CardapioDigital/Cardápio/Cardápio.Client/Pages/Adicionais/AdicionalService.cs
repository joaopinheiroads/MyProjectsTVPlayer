using Cardápio.Client.Dto;
using Cardápio.Client.Infra.Provider;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Cardápio.Client.Pages.Adicionais
{
    public class AdicionalService
    {
        private HttpClient _httpClient;
        private CustomAuthenticationStateProvider _provider;

        public AdicionalService(HttpClient httpClient, CustomAuthenticationStateProvider provider)
        {
            _httpClient = httpClient;
            _provider = provider;
        }

        public async Task<List<GrupoAdicionalGetDTO>> GetData(int companyID)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", await _provider.GetTokenAsync());

                List<GrupoAdicionalGetDTO> result = await _httpClient.GetFromJsonAsync<List<GrupoAdicionalGetDTO>>("/api/additional-group/data/" + companyID);

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return null;
        }

        public async Task<List<GrupoAdicionalGetDTO>> GetDataWithProducts(int companyID)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", await _provider.GetTokenAsync());

                List<GrupoAdicionalGetDTO> result = await _httpClient.GetFromJsonAsync<List<GrupoAdicionalGetDTO>>("/api/additional-group/data/" + companyID + "/with-product");

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return null;
        }

        public async Task OnDeleteAdditionalConfirm(int grupoID, int companyID)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", await _provider.GetTokenAsync());

                var response = await _httpClient.DeleteAsync("/api/additional-group/data/" + grupoID + "/" + companyID);
            }
            catch (Exception err)
            {
                Console.WriteLine(err.Message);
            }
        }
    }
}
