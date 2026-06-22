using Cardápio.Client.Dto;
using Cardápio.Client.Infra.Provider;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using SixLabors.ImageSharp;
using System.Collections.ObjectModel;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace Cardápio.Client.Pages.Empresas
{
    public class EmpresaService
    {
        private HttpClient _httpClient;
        private CustomAuthenticationStateProvider _provider;

        public EmpresaService(HttpClient httpClient, CustomAuthenticationStateProvider provider)
        {
            _httpClient = httpClient;
            _provider = provider;
        }

        public async Task<List<EmpresaGetDTO>> GetData()
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", await _provider.GetTokenAsync());

                List<EmpresaGetDTO> result = await _httpClient.GetFromJsonAsync<List<EmpresaGetDTO>>("/api/enterprise/GetEnterprises");

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return null;
        }

        public async Task<UploadResponseEmpresa> SaveFileToServer(Stream bannerStream, Stream logoStream)
        {
            if (bannerStream == null && logoStream == null)
            {
                return null;
            }

            MultipartFormDataContent formData = new MultipartFormDataContent();

            if (bannerStream != null)
            {
                bannerStream.Position = 0;
                var memoryStream = new MemoryStream();
                await bannerStream.CopyToAsync(memoryStream);

                memoryStream.Position = 0;
                formData.Add(new StreamContent(bannerStream), "banner", "banner.jpg");
            }

            if (logoStream != null)
            {
                logoStream.Position = 0;
                StreamContent logoContent = new StreamContent(logoStream);
                logoContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
                formData.Add(logoContent, "logo", "logo.jpg");
            }

            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", await _provider.GetTokenAsync());

            HttpResponseMessage response = await _httpClient.PostAsync("/api/enterprise/SaveCompanyImages", formData);

            string jsonResponse = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Failed to upload file. Status code: {response.StatusCode}, Reason: {jsonResponse}");
            }

            UploadResponseEmpresa result = System.Text.Json.JsonSerializer.Deserialize<UploadResponseEmpresa>(jsonResponse);

            return result;
        }

        public List<ImageFileAddDTO> InsertFileBanner(UploadResponseEmpresa files)
        {
            if (files != null && files.FileBanner != null && files.FileBanner.Length > 0)
            {
                return new List<ImageFileAddDTO>
                {
                    new ImageFileAddDTO
                    {
                        Nome = "image-banner",
                        Arquivo = files.FileBanner
                    }
                };
            }

            return new List<ImageFileAddDTO>
                {
                    new ImageFileAddDTO
                    {
                        Nome = "banner_default",
                        Arquivo = "banner_default.png"
                    }
                };
        }

        public List<ImageFileAddDTO> InsertFileLogo(UploadResponseEmpresa files)
        {
            if (files != null && files.FileLogo != null && files.FileLogo.Length > 0)
            {
                return new List<ImageFileAddDTO>
                {
                    new ImageFileAddDTO
                    {
                        Nome = "image-logo",
                        Arquivo = files.FileLogo
                    }
                };
            }

            return new List<ImageFileAddDTO>
                {
                    new ImageFileAddDTO
                    {
                        Nome = "logo_default",
                        Arquivo = "logo_default.png"
                    }
                };
        }

        public async Task OnCreateCompanyConfirm(EmpresaAddDTO companyTransferDataDTO)
        {
            try
            {
                companyTransferDataDTO.DiasDemo = 10;
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", await _provider.GetTokenAsync());
                HttpResponseMessage response = await _httpClient.PostAsJsonAsync("/api/enterprise/RegisterCompany", companyTransferDataDTO);

                var conteudo = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[RegisterCompany] Status: {response.StatusCode} | Resposta: {conteudo}");

                if (!response.IsSuccessStatusCode)
                    throw new Exception($"Status: {response.StatusCode} | Detalhe: {conteudo}");
            }
            catch (Exception err)
            {
                throw err;
            }
        }

        public async Task OnCreateCompanyInGroupIDConfirm(EmpresaAddDTO companyTransferDataDTO, int grupoID)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", await _provider.GetTokenAsync());
                HttpResponseMessage response = await _httpClient.PostAsJsonAsync("/api/enterprise/RegisterCompanyInGroupID/" + grupoID, companyTransferDataDTO);

                var conteudo = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[RegisterCompanyInGroupID] Status: {response.StatusCode} | Resposta: {conteudo}");

                if (!response.IsSuccessStatusCode)
                    throw new Exception($"Status: {response.StatusCode} | Detalhe: {conteudo}");
            }
            catch (Exception err)
            {
                throw err;
            }
        }

        public async Task OnEditCompanyConfirm(EmpresaAddDTO companyTransferDataDTO, int companyID)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", await _provider.GetTokenAsync());

                HttpResponseMessage response = await _httpClient.PutAsJsonAsync($"api/enterprise/EditCompany/{companyID}", companyTransferDataDTO);

                if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    string errorContent = await response.Content.ReadAsStringAsync();

                    throw new Exception(errorContent);
                }

                response.EnsureSuccessStatusCode();
            }
            catch (Exception err)
            {
                Console.WriteLine($"Erro: {err.Message}");
                Console.WriteLine($"Detalhes: {err.StackTrace}");

                throw err;
            }
        }

        public async Task OnDeleteCompanyConfirm(int companyID)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", await _provider.GetTokenAsync());

                HttpResponseMessage response = await _httpClient.DeleteAsync($"/api/enterprise/DeleteCompany/{companyID}");

                string? jsonResponse = await response.Content.ReadAsStringAsync();
            }
            catch (Exception err)
            {
                throw err;
            }
        }

        public async Task OnEditDateConfirm(int companyID, DateDTO dateDTO)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", await _provider.GetTokenAsync());

                HttpResponseMessage response = await _httpClient.PatchAsJsonAsync("/api/enterprise/EditDateDemo/" + companyID, dateDTO);

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

        public async Task<ObservableCollection<EmpresaGetDTO>> GetDataModalByGrupoID(int grupoID)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", await _provider.GetTokenAsync());
                return await _httpClient.GetFromJsonAsync<ObservableCollection<EmpresaGetDTO>>(
                    "/api/enterprise/GetEnterprisesByGrupoID/" + grupoID);
            }
            catch (Exception err)
            {
                throw err;
            }
        }

        public async Task<ObservableCollection<EmpresaGetDTO>> GetDataModal(UsuarioGetDTO selectedDTO, UsuarioGetDTO usuario)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", await _provider.GetTokenAsync());

                ObservableCollection<EmpresaGetDTO> result = null;

                if (selectedDTO != null && usuario.UsuarioTipoID == 4)
                {
                    result = await _httpClient.GetFromJsonAsync<ObservableCollection<EmpresaGetDTO>>("/api/enterprise/GetEnterprisesByUserID/" + selectedDTO.ID);
                }
                else
                {
                    result = await _httpClient.GetFromJsonAsync<ObservableCollection<EmpresaGetDTO>>("/api/enterprise/GetEnterprises");
                }

                return result;
            }
            catch (Exception err)
            {
                throw err;
            }
        }
    }

    public class UploadResponseEmpresa
    {
        [JsonPropertyName("fileBanner")]
        public string FileBanner { get; set; }

        [JsonPropertyName("fileLogo")]
        public string FileLogo { get; set; }
    }
}
