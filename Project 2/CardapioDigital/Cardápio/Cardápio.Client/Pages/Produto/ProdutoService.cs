using Cardápio.Client.Dto;
using Cardápio.Client.Infra.Provider;
using Cardápio.Client.Pages;
using System.Collections.ObjectModel;
using System.ComponentModel.Design;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace Cardápio.Client.Pages.Produto
{
    public class ProdutoService
    {
        private HttpClient _httpClient;
        private CustomAuthenticationStateProvider _provider;

        public ProdutoService(HttpClient httpClient, CustomAuthenticationStateProvider provider)
        {
            _httpClient = httpClient;
            _provider = provider;
        }

        public async Task<ObservableCollection<ProdutoGetDTO>> GetData(int companyID)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", await _provider.GetTokenAsync());

                ObservableCollection<ProdutoGetDTO> result = await _httpClient.GetFromJsonAsync<ObservableCollection<ProdutoGetDTO>>("/api/product/GetProductsByCompanyID/" + companyID);

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return null;
        }

        public async Task OnCreateProductConfirm(ProdutoAddDTO produto, int companyID)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", await _provider.GetTokenAsync());
                

                HttpResponseMessage result = await _httpClient.PostAsJsonAsync("/api/product/SaveProduct/" + companyID, produto);

                if (result.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    string errorContent = await result.Content.ReadAsStringAsync();
                    throw new Exception(errorContent);
                }

                result.EnsureSuccessStatusCode();
            }
            catch (Exception err)
            {
                Console.WriteLine(err);
                throw err;
            }
        }

        public async Task OnExportProductsConfirm(ExportarProdutosDTO products)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", await _provider.GetTokenAsync());

                HttpResponseMessage result = await _httpClient.PostAsJsonAsync("/api/product/ExportProducts/", products);

                result.EnsureSuccessStatusCode();
            }
            catch (Exception err)
            {
                Console.WriteLine(err);
                throw err;
            }
        }

        public async Task OnEditingProductConfirm(ProdutoAddDTO produto, int productID)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", await _provider.GetTokenAsync());

                HttpResponseMessage result = await _httpClient.PutAsJsonAsync("/api/product/EditProduct/" + productID, produto);

                if (result.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    string errorContent = await result.Content.ReadAsStringAsync();
                    throw new Exception(errorContent);
                }
            }
            catch (Exception err)
            {
                throw err;
            }
        }

        public async Task<ProdutoGetDTO> GetProductByID(int productID)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", await _provider.GetTokenAsync());

                ProdutoGetDTO result = await _httpClient.GetFromJsonAsync<ProdutoGetDTO>("/api/product/GetProductByID/" + productID);
                return result;
            }
            catch (Exception err)
            {
                throw err;
            }
        }

        public async Task OnDeleteProductConfirm(int productID, int companyID)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", await _provider.GetTokenAsync());

                var response = await _httpClient.DeleteAsync("/api/product/DeleteProduct/" + productID + "/" + companyID);
            }
            catch (Exception err)
            {
                Console.WriteLine(err.Message);
            }
        }
    }

    public class UploadResponse
    {
        [JsonPropertyName("fileName")]
        public string FileName { get; set; }
    }
}
