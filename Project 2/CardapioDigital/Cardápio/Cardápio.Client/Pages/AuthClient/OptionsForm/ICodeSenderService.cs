namespace Cardápio.Client.Pages.AuthClient.OptionsForm
{
    public interface ICodeSenderService
    {
        Task SendCodeAsync(string destination);
        Task<HttpResponseMessage> VerifyCodeAsync(string destination, string code);
    }
}
