using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using TVPlayerSite.APIClient;
using TVPlayerSite.Configuration;
using TVPlayerSite.Disparos;
using TVPlayerSite.Helpers;
using TVPlayerSite.Models.CRM;

namespace TVPlayerSite.Services.Demonstracao
{
  
    public sealed class ResultadoCadastro
    {
        public bool CadastroSucesso { get; set; }
        public bool EmailSucesso { get; set; }
    }

   
    public interface ICadastroClienteService
    {
        Task<ResultadoCadastro> ProcessarAsync(TabCliente cliente);
    }

    public sealed class CadastroClienteService : ICadastroClienteService
    {
        private readonly ApiClient _apiClient;
        private readonly IEmailService _emailService;
        private readonly IDisparoMensagem _disparoMensagem;
        private readonly IConfiguration _configuration;
        private readonly DemonstracaoOptions.LicencaDefaults _licenca;
        private readonly IDadosService _dadosService;
        private readonly IVerificacaoLogger _logger;

        public CadastroClienteService(
            ApiClient apiClient,
            IEmailService emailService,
            IDisparoMensagem disparoMensagem,
            IConfiguration configuration,
            IOptions<DemonstracaoOptions> options,
            IDadosService dadosService,
            IVerificacaoLogger logger)
        {
            _apiClient = apiClient;
            _emailService = emailService;
            _disparoMensagem = disparoMensagem;
            _configuration = configuration;
            _licenca = options.Value.Licenca;
            _dadosService = dadosService;
            _logger = logger;
        }

        public async Task<ResultadoCadastro> ProcessarAsync(TabCliente cliente)
        {
       
            cliente.CliTipoId = _licenca.CliTipoId;
            string chave = Guid.NewGuid().ToString();

            var licenca = new TabLicenca
            {
                LicChave = chave,
                LicValidade = _licenca.Validade,
                LicDataCadastro = DateTime.Now,
                LicUnidadeId = _licenca.UnidadeId,
                LicUsuarioIdcadastro = _licenca.UsuarioIdCadastro,
                LicTipoDeLicencaId = _licenca.TipoDeLicencaId
            };

            cliente.TabLicenca.Add(licenca);

            Usuario usuarioExistente = null;
            try
            {
                usuarioExistente = await _apiClient.GetUsuario(cliente.CliEmail);
            }
            catch (Exception)
            {
                
            }

            return usuarioExistente != null
                ? await ProcessarClienteExistenteAsync(cliente, usuarioExistente)
                : await ProcessarClienteNovoAsync(cliente, licenca);
        }

        private async Task<ResultadoCadastro> ProcessarClienteExistenteAsync(TabCliente cliente, Usuario usuarioExistente)
        {
            bool result;
            bool emailSucesso = false;
            string emailErro = null;

            try
            {
                var resultadoEmail = _emailService.EnviarDemonstracaoExistente(usuarioExistente);
                result = resultadoEmail.sucesso;
                emailSucesso = resultadoEmail.sucesso;
                emailErro = resultadoEmail.erro;

                int idUsuario = usuarioExistente.ID;
                var link = EncryptHelper.Encrypt(idUsuario.ToString());

                string nomeEmpresa = _dadosService.FormatarNomeEmpresa(cliente.CliEmpresa);

                string nome = cliente.CliNome;
                string clienteNome = _dadosService.FormatarNomePessoa(nome);

                string numeroDestino = _dadosService.NormalizarBrasil(cliente.CliTelefoneCelular);

                await _disparoMensagem.EnviarTemplateComBotaoUrlAsync(numeroDestino, _configuration["Digisac:UserExistTemplateId"], link, 0, clienteNome, nomeEmpresa);

                if (!result && string.IsNullOrEmpty(emailErro))
                    emailErro = "Falha no envio de email para usuário existente (erro desconhecido)";
            }
            catch (Exception ex)
            {
                result = false;
                emailSucesso = false;
                emailErro = ex.Message;
            }

            return new ResultadoCadastro { CadastroSucesso = result, EmailSucesso = emailSucesso };
        }

        private async Task<ResultadoCadastro> ProcessarClienteNovoAsync(TabCliente cliente, TabLicenca licenca)
        {
            bool result;
            bool emailSucesso = false;
            string emailErro = null;

          
            TabCliente newDemonstracao;
            try
            {
                newDemonstracao = await _apiClient.SaveCliente(cliente);
            }
            catch (Exception ex)
            {
                await _logger.LogDisparoEmail(cliente.CliEmail, "CONFIRMACAO", false, $"Exceção ao salvar cliente: {ex.Message}");
                return new ResultadoCadastro { CadastroSucesso = false, EmailSucesso = false };
            }

            if (newDemonstracao == null)
            {
                emailErro = "Falha ao salvar cliente";

               
                await _logger.LogDisparoEmail(cliente.CliEmail, "CONFIRMACAO", false, emailErro);

                return new ResultadoCadastro { CadastroSucesso = false, EmailSucesso = false };
            }

            string linkConfirmacao = $"?p={licenca.LicChave}" + $"&e={HexadecimalEncoding.ToHexString(cliente.CliEmail)}";
            cliente.CliLinkConfirmacao = linkConfirmacao;

            try
            {
                await _apiClient.AtualizarLinkConfirmacao(newDemonstracao.CliId, linkConfirmacao);
            }
            catch (Exception)
            {
              
            }

            try
            {
                var resultadoEmail = _emailService.EnviarConfirmacaoCadastro(cliente);

                result = resultadoEmail.sucesso;
                emailSucesso = resultadoEmail.sucesso;
                emailErro = resultadoEmail.erro;

                if (!result && string.IsNullOrEmpty(emailErro))
                    emailErro = "Falha no envio de email de confirmação (erro desconhecido)";
            }
            catch (Exception ex)
            {
                result = false;
                emailSucesso = false;
                emailErro = ex.Message;
            }

          
            bool whatsappBoasVindasSucesso = false;
            string whatsappBoasVindasErro = null;

            string nome = cliente.CliNome;
            string clienteNome = _dadosService.FormatarNomePessoa(nome);

            var numeroDestinoDois = _dadosService.NormalizarBrasil(cliente.CliTelefoneCelular);

            try
            {
                
                string queryConfirmacao = $"?p={licenca.LicChave}" +
                                          $"&e={HexadecimalEncoding.ToHexString(cliente.CliEmail)}";

            
                string empresa = string.IsNullOrWhiteSpace(cliente.CliEmpresa) ? "-" : _dadosService.FormatarNomeEmpresa(cliente.CliEmpresa);

                var resultadoMensagemDois = await _disparoMensagem.EnviarTemplateComBotaoUrlAsync(numeroDestinoDois, _configuration["Digisac:ConfirmEmailTemplateId"], queryConfirmacao, 0, clienteNome, empresa);

                if (resultadoMensagemDois.Sucesso)
                {
                    whatsappBoasVindasSucesso = true;
                }
                else
                {
                    whatsappBoasVindasSucesso = false;
                    whatsappBoasVindasErro = resultadoMensagemDois.Erro ?? "Falha no envio do WhatsApp de boas-vindas (erro desconhecido)";
                }
            }
            catch (Exception ex)
            {
                whatsappBoasVindasSucesso = false;
                whatsappBoasVindasErro = ex.Message;
            }

            
            await _logger.LogDisparoWhatsApp(
                cliente.CliTelefoneCelular,
                "BOAS_VINDAS",
                whatsappBoasVindasSucesso,
                whatsappBoasVindasErro
            );

            await _logger.LogDisparoEmail(
                cliente.CliEmail,
                "CONFIRMACAO",
                emailSucesso,
                emailErro
            );

           
            if (result)
            {
                await _logger.LogSucessoCadastro(
                    cliente.CliNome,
                    cliente.CliTelefoneCelular,
                    cliente.CliEmail
                );
            }

            return new ResultadoCadastro { CadastroSucesso = result, EmailSucesso = emailSucesso };
        }
    }
}
