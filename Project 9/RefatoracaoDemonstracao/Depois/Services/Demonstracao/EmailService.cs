using Microsoft.AspNetCore.Hosting;
using TVPlayerSite.Helpers;
using TVPlayerSite.Models.CRM;

namespace TVPlayerSite.Services.Demonstracao
{
    // Abstração dos e-mails do fluxo de demonstração.
    // Antes o controller fazia `new EmailHelper()` (dependência concreta) e ainda montava
    // "à mão" o caminho do template .txt (WebRootPath + nome do arquivo) em cada action.
    // Agora ele só declara a INTENÇÃO ("enviar confirmação de cadastro") e o serviço cuida
    // de onde está o template e de qual método do EmailHelper chamar (SRP + DIP).
    public interface IEmailService
    {
        (bool sucesso, string erro) EnviarDemonstracaoExistente(Usuario usuario);
        (bool sucesso, string erro) EnviarConfirmacaoCadastro(TabCliente cliente);
        (bool sucesso, string erro) EnviarProcedimentosInstalacao(UsuarioInfo usuario);
    }

    public sealed class EmailService : IEmailService
    {
        // Nomes dos templates ficam num único lugar (antes estavam espalhados nas actions).
        private const string TemplateCadastroExistente = "cadastroExistente.txt";
        private const string TemplateConfirmarEmail = "confirmarEmail.txt";
        private const string TemplateProcedimentos = "procedimentos.txt";

        private readonly IHostingEnvironment _env;

        public EmailService(IHostingEnvironment env) => _env = env;

        public (bool sucesso, string erro) EnviarDemonstracaoExistente(Usuario usuario)
            => new EmailHelper().EnviarEmailDemonstracaoExistenteComDetalhes(Caminho(TemplateCadastroExistente), usuario);

        public (bool sucesso, string erro) EnviarConfirmacaoCadastro(TabCliente cliente)
            => new EmailHelper().EnviarEmailSolicitacaoDemonstracaoComDetalhes(Caminho(TemplateConfirmarEmail), cliente);

        public (bool sucesso, string erro) EnviarProcedimentosInstalacao(UsuarioInfo usuario)
            => new EmailHelper().EnviarEmailProcedimentoInstalacaoComDetalhes(Caminho(TemplateProcedimentos), usuario);

        private string Caminho(string template) => $"{_env.WebRootPath}/email/txt/{template}";
    }
}
