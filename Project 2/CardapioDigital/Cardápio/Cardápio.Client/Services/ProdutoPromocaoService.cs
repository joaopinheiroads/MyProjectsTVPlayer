using Cardápio.Client.Dto;
using System.Text.Json;
using System.Text;

namespace Cardápio.Client.Services
{
    public class ProdutoPromocaoService
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions;

        public ProdutoPromocaoService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        public async Task<bool> ConfigurarPromocoesProdutoAsync(int produtoId, List<ProdutoPromocaoHorarioAddDTO> promocoes)
        {
            try
            {
                var configuracao = new ConfiguracaoPromocaoSemanalDTO
                {
                    ProdutoId = produtoId,
                    Promocoes = promocoes
                };

                var json = JsonSerializer.Serialize(configuracao, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("/api/ProdutoPromocao/configurar", content);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao configurar promoções: {ex.Message}");
                return false;
            }
        }

        public async Task<List<ProdutoPromocaoHorarioGetDTO>> ObterPromocoesProdutoAsync(int produtoId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/ProdutoPromocao/produto/{produtoId}");
                
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<List<ProdutoPromocaoHorarioGetDTO>>(json, _jsonOptions) ?? new List<ProdutoPromocaoHorarioGetDTO>();
                }
                
                return new List<ProdutoPromocaoHorarioGetDTO>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao obter promoções: {ex.Message}");
                return new List<ProdutoPromocaoHorarioGetDTO>();
            }
        }

        public async Task<decimal?> ObterPrecoAtualAsync(int produtoId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/ProdutoPromocao/preco-atual/{produtoId}");
                
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var resultado = JsonSerializer.Deserialize<PrecoAtualResultDTO>(json, _jsonOptions);
                    return resultado?.PrecoAtual;
                }
                
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao obter preço atual: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> RemoverPromocaoAsync(int promocaoId)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"/api/ProdutoPromocao/{promocaoId}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao remover promoção: {ex.Message}");
                return false;
            }
        }

        public async Task<List<ProdutoPromocaoHorarioGetDTO>> ObterPromocoesAtivasAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("/api/ProdutoPromocao/ativas");
                
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<List<ProdutoPromocaoHorarioGetDTO>>(json, _jsonOptions) ?? new List<ProdutoPromocaoHorarioGetDTO>();
                }
                
                return new List<ProdutoPromocaoHorarioGetDTO>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao obter promoções ativas: {ex.Message}");
                return new List<ProdutoPromocaoHorarioGetDTO>();
            }
        }
    }

    // DTOs para o service
    public class ConfiguracaoPromocaoSemanalDTO
    {
        public int ProdutoId { get; set; }
        public List<ProdutoPromocaoHorarioAddDTO> Promocoes { get; set; } = new();
    }

    public class PrecoAtualResultDTO
    {
        public decimal? PrecoAtual { get; set; }
        public bool TemPromocaoAtiva { get; set; }
        public string? PromocaoAtiva { get; set; }
    }
}
