using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace EscolhaAI.Client.Services
{
    public class NavigationService
    {
        private readonly NavigationManager _nav;
        private readonly IJSRuntime _js;

        public NavigationService(NavigationManager nav, IJSRuntime js)
        {
            _nav = nav;
            _js = js;
        }

        public void GoHome()
        {
            _nav.NavigateTo("/");
        }

        public void GoExternal(string url)
        {
            _nav.NavigateTo(url, forceLoad: true);
        }

        public async Task OpenNewTab(string url)
        {
            await _js.InvokeVoidAsync("window.open", url, "_blank");
        }



        public async Task RedirectForLogin()
        {
            await _js.InvokeVoidAsync(
                "window.open",
                "https://app.escolha.ai:44303/login",
                "_blank"
            );
        }

        public async Task RedirectForCardapio()
        {
            await _js.InvokeVoidAsync(
                "window.open",
                "https://app.escolha.ai:44303/loja/Dig-Pizza-Regi%C3%A3o-Sul",
                "_blank"
            );
            
        }

        public async Task RedirectForCardapioTwoo()
        {
            await _js.InvokeVoidAsync(
                "window.open",
                "https://app.escolha.ai:44303/loja/Rei-do-Hamburguer-Regi%C3%A3o-Leste",
                "_blank"
            );
        }

       
    }

    



}
