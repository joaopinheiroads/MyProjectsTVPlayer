using Cardápio.Client.Dto;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Cardápio.Client.Infra.Provider
{
    public class CustomAuthenticationStateProvider
    {
        private readonly IJSRuntime _jsRuntime;
        private readonly NavigationManager _navigationManager;
        private readonly HttpClient _httpClient;
        public UsuarioClienteGetDTO UsuarioClienteGetDTO { get; set; }
        public UsuarioGetDTO UsuarioOwner { get; set; }
        public bool IsLoading { get; set; } = true;

        public CustomAuthenticationStateProvider(IJSRuntime jsRuntime, NavigationManager navigationManager, HttpClient httpClient)
        {
            _jsRuntime = jsRuntime;
            _navigationManager = navigationManager;
            _httpClient = httpClient;
        }

        public async Task VerifyTokenIsPresent()
        {
            try
            {
                string token = await _jsRuntime.InvokeAsync<string>("eval", "document.cookie.split('; ').find(row => row.startsWith('jwt=')).split('=')[1]");

                if (IsValidToken(token))
                {
                    await Task.Yield();
                    _navigationManager.NavigateTo("/categorias", true);
                }
            }
            catch
            {
            }
        }

        public async Task<UsuarioClienteGetDTO?> GetClientUser()
        {
            try
            {
                var cookie = await _jsRuntime.InvokeAsync<string>("eval", "document.cookie.split('; ').find(row => row.startsWith('jwt='))");

                if (string.IsNullOrEmpty(cookie))
                {
                    return null;
                }

                var token = cookie.Split('=')[1];

                if (string.IsNullOrEmpty(token) || !IsValidToken(token))
                {
                    return null;
                }

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                UsuarioClienteGetDTO = await _httpClient.GetFromJsonAsync<UsuarioClienteGetDTO>("api/authentication/client-user");

                return UsuarioClienteGetDTO;
            }
            catch
            {
                return null;
            } finally
            {
                IsLoading = false;
            }
        }

        public async Task<UsuarioGetDTO?> GetOwnerUser()
        {
            try
            {
                var cookie = await _jsRuntime.InvokeAsync<string>("eval", "document.cookie.split('; ').find(row => row.startsWith('jwt='))");

                if (string.IsNullOrEmpty(cookie))
                {
                    return null;
                }

                var token = cookie.Split('=')[1];

                if (string.IsNullOrEmpty(token) || !IsValidToken(token))
                {
                    return null;
                }

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                UsuarioOwner = await _httpClient.GetFromJsonAsync<UsuarioGetDTO>("api/authentication/getCompanyID");
                IsLoading = false;

                return UsuarioOwner;
            }
            catch
            {
                return null;
            } finally
            {
                IsLoading = false;
            }
        }

        public async Task<string> GetTokenAsyncIsNotRequired()
        {
            try
            {
                string token = await _jsRuntime.InvokeAsync<string>("eval", "document.cookie.split('; ').find(row => row.startsWith('jwt=')).split('=')[1]");

                if (!IsValidToken(token))
                {
                    return null;
                }

                return token;
            }
            catch
            {
                return null;
            }
        }

        public async Task<string> GetTokenAsync()
        {
            try
            {
                string token = await _jsRuntime.InvokeAsync<string>("eval", "document.cookie.split('; ').find(row => row.startsWith('jwt=')).split('=')[1]");

                if (!IsValidToken(token))
                {
                    _navigationManager.NavigateTo("/login", true);
                    return null;
                }

                return token;
            }
            catch
            {
                _navigationManager.NavigateTo("/login", true);
                return null;
            }
        }

        private bool IsValidToken(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return false;
            }

            var partes = token.Split('.');
            return partes.Length == 3;
        }

        public async Task SetTokenAsync(string token, bool keepLoggedIn)
        {
            try
            {
                if (!string.IsNullOrEmpty(token))
                {
                    if (keepLoggedIn)
                    {
                        await _jsRuntime.InvokeVoidAsync("eval", $@"
                document.cookie = 'jwt={token}; path=/; secure; SameSite=Strict; expires={DateTime.UtcNow.AddDays(7).ToString("R")}';
                ");
                    }
                    else
                    {
                        await _jsRuntime.InvokeVoidAsync("eval", $@"
                document.cookie = 'jwt={token}; path=/; secure; SameSite=Strict';
                ");
                    }
                }

            }
            catch
            {
                _navigationManager.NavigateTo("/login", true);
            }
        }

        public async Task LogoutAsync()
        {
            try
            {
                await _jsRuntime.InvokeVoidAsync("eval", "document.cookie = 'jwt=; expires=Thu, 01 Jan 1970 00:00:00 GMT; path=/;';");
            }
            catch
            {
                _navigationManager.NavigateTo("/login", true);
            }
        }
    }
}