using Cardápio.Client.Dto;
using System.Net.Http.Json;

namespace Cardápio.Client.Services
{
    public class ProdutoHorarioService
    {
        private readonly HttpClient _httpClient;

        public ProdutoHorarioService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<ProdutoHorarioGetDTO>> GetHorariosByProdutoId(int produtoId)
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<List<ProdutoHorarioGetDTO>>($"api/ProdutoHorario/produto/{produtoId}");
                return response ?? new List<ProdutoHorarioGetDTO>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao buscar horários do produto {produtoId}: {ex.Message}");
                return new List<ProdutoHorarioGetDTO>();
            }
        }

        public async Task<bool> IsProdutoDisponivelNoHorario(int produtoId)
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<dynamic>($"api/ProdutoHorario/produto/{produtoId}/disponivel");
                return response?.disponivel ?? true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao verificar disponibilidade do produto {produtoId}: {ex.Message}");
                return true; // Por segurança, retorna true se houver erro
            }
        }

        public async Task<ProdutoHorarioGetDTO> AddHorario(ProdutoHorarioAddDTO horario)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/ProdutoHorario", horario);
                
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ProdutoHorarioGetDTO>();
                    return result ?? throw new Exception("Resposta inválida do servidor");
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new Exception($"Erro ao adicionar horário: {errorContent}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao adicionar horário: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> DeleteHorario(int horarioId)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"api/ProdutoHorario/{horarioId}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao deletar horário {horarioId}: {ex.Message}");
                throw;
            }
        }

        public async Task<ProdutoHorarioGetDTO> UpdateHorario(int horarioId, ProdutoHorarioAddDTO horario)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"api/ProdutoHorario/{horarioId}", horario);
                
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ProdutoHorarioGetDTO>();
                    return result ?? throw new Exception("Resposta inválida do servidor");
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new Exception($"Erro ao atualizar horário: {errorContent}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao atualizar horário {horarioId}: {ex.Message}");
                throw;
            }
        }
    }
}
