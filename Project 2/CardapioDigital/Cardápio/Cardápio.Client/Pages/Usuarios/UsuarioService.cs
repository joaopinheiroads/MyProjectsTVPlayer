using Cardápio.Client.Dto;
using Cardápio.Client.Infra.Provider;
using System.Collections.ObjectModel;
using System.Net.Http.Json;

namespace Cardápio.Client.Pages.Usuarios
{
    public class UsuarioService
    {
        private HttpClient _httpClient;
        private CustomAuthenticationStateProvider _provider;

        public UsuarioService(HttpClient httpClient, CustomAuthenticationStateProvider provider)
        {
            _httpClient = httpClient;
            _provider = provider;
            
        }

        public async Task<ObservableCollection<UsuarioGetDTO>> GetData()
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", await _provider.GetTokenAsync());

                ObservableCollection<UsuarioGetDTO> result = await _httpClient.GetFromJsonAsync<ObservableCollection<UsuarioGetDTO>>("/api/users/GetUsers");

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        public async Task OnCreateProductConfirm(UsuarioAddDTO userDTO)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", await _provider.GetTokenAsync());

                // pega o grupoID do usuário logado
                UsuarioGetDTO usuarioLogado = await _httpClient.GetFromJsonAsync<UsuarioGetDTO>("/api/authentication/getCompanyID");

                int grupoID = usuarioLogado.GrupoID ?? 0;

                await _httpClient.PostAsJsonAsync($"/api/users/registrarInCompanyID/{grupoID}", userDTO);
            }
            catch (Exception err)
            {
                throw err;
            }
        }

        public async Task OnCreateUserInCompanyIDConfirm(UsuarioAddDTO userDTO, int groupID)
        {
            try
            {
                Console.WriteLine($"[OnCreateUserInCompanyIDConfirm] groupID: {groupID}, EmpresaID: {userDTO.EmpresaID}");

                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", await _provider.GetTokenAsync());

                HttpResponseMessage result = await _httpClient.PostAsJsonAsync($"/api/users/registrarInCompanyID/{groupID}", userDTO);

                // ✅ Verifica se deu erro e lança a mensagem correta
                if (!result.IsSuccessStatusCode)
                {
                    var erro = await result.Content.ReadAsStringAsync();
                    throw new Exception(erro);
                }
            }
            catch (Exception err)
            {
                throw err;
            }
        }

        public async Task OnEditProductConfirm(UsuarioUpdateDTO userDTO, int selectedUserID)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", await _provider.GetTokenAsync());

                await _httpClient.PutAsJsonAsync("/api/users/editUser/" + selectedUserID, userDTO);
            }
            catch (Exception err)
            {
                throw err;
            }
        }

        public async Task OnDeleteUserConfirm(int selectedUserID)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", await _provider.GetTokenAsync());

                await _httpClient.DeleteAsync("/api/users/deleteUser/" + selectedUserID);
            }
            catch (Exception err)
            {
                throw err;
            }
        }

        public async Task EnviarCodigoRecuperacao(string email)
        {
            var response = await _httpClient.PostAsJsonAsync("api/users/enviar-codigo", new { email });

            if (!response.IsSuccessStatusCode)
            {
                var erro = await response.Content.ReadAsStringAsync();
                throw new Exception($"Status: {response.StatusCode} | Detalhe: {erro}");
            }
        }

        public async Task TrocarSenha(string email, string novaSenha, string codigo)
        {
            var response = await _httpClient.PostAsJsonAsync("api/users/trocarSenha",
                new { email, novaSenha, codigoVerificacao = codigo });

            if (!response.IsSuccessStatusCode)
            {
                var erro = await response.Content.ReadAsStringAsync();
                throw new Exception($"Erro ao trocar senha. Detalhes: {erro}");
            }
        }
    }
}
