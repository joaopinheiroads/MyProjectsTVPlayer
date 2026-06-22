using Cardápio.Client.Dto;
using System.Net.Http.Json;

namespace Cardápio.Client.Services
{
    public class PromocaoService
    {
        private readonly HttpClient _httpClient;

        public PromocaoService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<PromocaoGetDTO>> GetPromocoesByEmpresa(int empresaId)
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<List<PromocaoGetDTO>>($"api/promocao/empresa/{empresaId}");
                return response ?? new List<PromocaoGetDTO>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao buscar promoções da empresa: {ex.Message}");
                return new List<PromocaoGetDTO>();
            }
        }

        public async Task<PromocaoGetDTO?> GetById(int id)
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<PromocaoGetDTO>($"api/promocao/{id}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao buscar promoção: {ex.Message}");
                return null;
            }
        }

        public async Task<PromocaoGetDTO?> Add(PromocaoAddDTO promocao)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/promocao", promocao);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<PromocaoGetDTO>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao criar promoção: {ex.Message}");
                return null;
            }
        }

        public async Task<PromocaoGetDTO?> Update(PromocaoUpdateDTO promocao)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"api/promocao/{promocao.ID}", promocao);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<PromocaoGetDTO>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao atualizar promoção: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> Delete(int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"api/promocao/{id}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao deletar promoção: {ex.Message}");
                return false;
            }
        }

        public async Task<List<PromocaoGetDTO>> GetPromocoesAtivas(int empresaId)
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<List<PromocaoGetDTO>>($"api/promocao/ativas/empresa/{empresaId}");
                return response ?? new List<PromocaoGetDTO>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao buscar promoções ativas: {ex.Message}");
                return new List<PromocaoGetDTO>();
            }
        }

        public async Task<decimal?> GetPrecoPromocionalProduto(int produtoId)
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<object>($"api/promocao/produto/{produtoId}/preco-promocional");
                if (response != null)
                {
                    var jsonElement = (System.Text.Json.JsonElement)response;
                    if (jsonElement.TryGetProperty("precoPromocional", out var precoProperty))
                    {
                        if (precoProperty.ValueKind == System.Text.Json.JsonValueKind.Number)
                        {
                            return precoProperty.GetDecimal();
                        }
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao buscar preço promocional: {ex.Message}");
                return null;
            }
        }
    }
}
