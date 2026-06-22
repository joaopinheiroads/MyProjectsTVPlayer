using Cardápio.Client.Dto;
using Cardápio.Client.Infra.Provider;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Cardápio.Client.Pages.Categoria
{
    public class CategoriaService
    {
        private HttpClient _httpClient;
        private CustomAuthenticationStateProvider _provider;

        public CategoriaService(HttpClient httpClient, CustomAuthenticationStateProvider provider)
        {
            _httpClient = httpClient;
            _provider = provider;
        }

        public async Task UpdateOrderAsync(List<CategoriaGetDTO> updatedOrders)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", await _provider.GetTokenAsync());

                await _httpClient.PutAsJsonAsync("/api/category/UpdateCategoriaOrdem", updatedOrders);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao atualizar a ordem: {ex.Message}");
            }
        }

        public async Task<List<CategoriaGetDTO>> GetData(int companyID)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", await _provider.GetTokenAsync());

                List<CategoriaGetDTO> result = await _httpClient.GetFromJsonAsync<List<CategoriaGetDTO>>("/api/category/GetCategoriesByCompanyID/" + companyID);

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return null;
        }

        public async Task OnCreateCategoryConfirm(CategoriaAddDTO category, int companyID)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", await _provider.GetTokenAsync());

                string requestUrl = $"/api/category/CreateCategory/{companyID}";

                HttpResponseMessage response = await _httpClient.PostAsJsonAsync(requestUrl, category);

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

        public async Task OnEditCategoryConfirm(CategoriaUpdateDTO category, int selectedCategoryID)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", await _provider.GetTokenAsync());

                HttpResponseMessage response = await _httpClient.PutAsJsonAsync("/api/category/updateCategory/" + selectedCategoryID, category);

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

        public async Task OnDeleteCategoryConfirm(int selectedCategoryID, int companyID)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", await _provider.GetTokenAsync());

                await _httpClient.DeleteAsync("/api/category/deleteCategory/" + selectedCategoryID + "/" + companyID);
            }
            catch (Exception err)
            {
                Console.WriteLine(err.Message);
            }
        }
    }
}
