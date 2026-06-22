using Microsoft.JSInterop;

namespace EscolhaAI.Client.Services
{
    public class WhatsAppService
    {
        private readonly IJSRuntime _js;


        public WhatsAppService(IJSRuntime js)
        {
            _js = js;
        }

        public async Task OpenWhatsApp(string phone, string message)
        {
            var url = $"https://wa.me/{phone}?text={Uri.EscapeDataString(message)}";

            await _js.InvokeVoidAsync(
                "window.open",
                url,
                "_blank"
            );
        }
    }
}
