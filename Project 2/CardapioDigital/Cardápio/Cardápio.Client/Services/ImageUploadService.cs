using Cardápio.Client.Dto;
using Cardápio.Client.Infra.Provider;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Cardápio.Client.Services
{
    public class ImageUploadService
    {
        private readonly HttpClient _httpClient;
        private readonly CustomAuthenticationStateProvider _provider;

        public ImageUploadService(HttpClient httpClient, CustomAuthenticationStateProvider provider)
        {
            _httpClient = httpClient;
            _provider = provider;
        }

        public async Task<ImageUploadResponseDTO> UploadTempImageAsync(Stream imageStream, string fileName)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", await _provider.GetTokenAsync());

                using var content = new MultipartFormDataContent();
                using var fileContent = new StreamContent(imageStream);
                fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
                content.Add(fileContent, "file", fileName);

                var response = await _httpClient.PostAsync("/api/image/upload-temp", content);
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<ImageUploadResponseDTO>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao fazer upload da imagem: {ex.Message}");
                throw;
            }
        }

        public async Task DeleteTempImageAsync(string imageId)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", await _provider.GetTokenAsync());

                var response = await _httpClient.DeleteAsync($"/api/image/delete-temp/{imageId}");
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao deletar imagem temporária: {ex.Message}");
                // Não propagar o erro, pois a remoção é opcional
            }
        }

        public async Task<ImageUploadResponseDTO> UploadAdditionalImageAsync(Stream imageStream, string fileName)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", await _provider.GetTokenAsync());

                using var content = new MultipartFormDataContent();
                using var fileContent = new StreamContent(imageStream);
                fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
                content.Add(fileContent, "file", fileName);

                var response = await _httpClient.PostAsync("/api/image/upload-additional", content);
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<ImageUploadResponseDTO>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao fazer upload da imagem de adicional: {ex.Message}");
                throw;
            }
        }
    }
}
