using System;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using TVPlayerSite.APIClient;
using TVPlayerSite.Disparos;
using TVPlayerSite.Helpers;
using TVPlayerSite.Models.CRM;

namespace TVPlayerSite.Services.Demonstracao
{
  

    public sealed class IniciarVerificacaoResultado
    {
        public string NomeCliente { get; set; }
        public string Telefone { get; set; }
        public bool WhatsappSucesso { get; set; }
        public string WhatsappErro { get; set; }
    }

    public sealed class VerificacaoCodigoResultado
    {
        public bool Sucesso { get; set; }
        public string Erro { get; set; }
        public bool Expirado { get; set; }
        public bool CadastroSucesso { get; set; }
        public bool EmailSucesso { get; set; }
        public string NomeCliente { get; set; }
    }

    public sealed class ReenvioResultado
    {
        public bool Sucesso { get; set; }
        public string Mensagem { get; set; }
        public string Telefone { get; set; }
    }

    public interface IDemonstracaoAppService
    {
        Task<IniciarVerificacaoResultado> IniciarVerificacaoAsync(TabCliente cliente);
        Task<VerificacaoCodigoResultado> VerificarCodigoAsync(string codigoInformado);
        Task<ReenvioResultado> ReenviarPorEmailAsync();
        Task<ReenvioResultado> ReenviarPorWhatsAppAsync();
        Task<string> ValidarEmailAsync(string chave, string email);
    }

    public sealed class DemonstracaoAppService : IDemonstracaoAppService
    {
        private readonly ApiClient _apiClient;
        private readonly IConfiguration _configuration;
        private readonly IDisparoMensagem _disparoMensagem;
        private readonly ICodigoVerificacaoService _codigoVerificacao;
        private readonly IVerificacaoSessionStore _sessaoVerificacao;
        private readonly IEmailService _emailService;
        private readonly ICadastroClienteService _cadastroCliente;
        private readonly IDadosService _dadosService;
        private readonly IVerificacaoLogger _logger;

        public DemonstracaoAppService(
            ApiClient apiClient,
            IConfiguration configuration,
            IDisparoMensagem disparoMensagem,
            ICodigoVerificacaoService codigoVerificacao,
            IVerificacaoSessionStore sessaoVerificacao,
            IEmailService emailService,
            ICadastroClienteService cadastroCliente,
            IDadosService dadosService,
            IVerificacaoLogger logger)
        {
            _apiClient = apiClient;
            _configuration = configuration;
            _disparoMensagem = disparoMensagem;
            _codigoVerificacao = codigoVerificacao;
            _sessaoVerificacao = sessaoVerificacao;
            _emailService = emailService;
            _cadastroCliente = cadastroCliente;
            _dadosService = dadosService;
            _logger = logger;
        }

        public async Task<IniciarVerificacaoResultado> IniciarVerificacaoAsync(TabCliente cliente)
        {
            await _logger.LogInicioProcesso(cliente.CliNome, cliente.CliTelefoneCelular, cliente.CliEmail);

            string codigoVerificacao = _codigoVerificacao.Gerar();

            _sessaoVerificacao.Salvar(cliente, codigoVerificacao, DateTime.Now);

            bool whatsappSucesso = false;
            string whatsappErro = null;

            try
            {
                string numeroDestino = _dadosService.NormalizarBrasil(cliente.CliTelefoneCelular);

                var resultadoEnvio = await _disparoMensagem.EnviarTemplateAsync(numeroDestino, _configuration["Digisac:CodeTemplateId"], true, codigoVerificacao);


                bool enviado = await _apiClient.EnviarCodigo(
                cliente.CliEmail,
                cliente.CliNome,
                codigoVerificacao
            );
                if (resultadoEnvio.Sucesso)
                {
                    whatsappSucesso = true;
                }
                else
                {
                    whatsappSucesso = false;
                    whatsappErro = resultadoEnvio.Erro ?? "Falha no envio via WhatsApp";
                }
            }
            catch (Exception ex)
            {
                whatsappSucesso = false;
                whatsappErro = ex.Message;
            }

            await _logger.LogDisparoWhatsApp(cliente.CliTelefoneCelular, codigoVerificacao, whatsappSucesso, whatsappErro);

            return new IniciarVerificacaoResultado
            {
                NomeCliente = cliente.CliNome,
                Telefone = cliente.CliTelefoneCelular,
                WhatsappSucesso = whatsappSucesso,
                WhatsappErro = whatsappErro
            };
        }

        public async Task<VerificacaoCodigoResultado> VerificarCodigoAsync(string codigoInformado)
        {
            if (!_sessaoVerificacao.TryObter(out var dadosSessao))
            {
                return new VerificacaoCodigoResultado { Sucesso = false, Erro = "Sessão expirada. Por favor, faça o cadastro novamente." };
            }

            string codigoCorreto = dadosSessao.Codigo;

            if (!_codigoVerificacao.EstaValido(dadosSessao.EmitidoEm))
            {
                await _logger.LogExpiracaoTempo(JsonConvert.SerializeObject(dadosSessao.Cliente));

                return new VerificacaoCodigoResultado { Sucesso = false, Erro = "Código expirado. Por favor, faça o cadastro novamente.", Expirado = true };
            }

            TabCliente clienteAtual = dadosSessao.Cliente;

            if (codigoInformado != codigoCorreto)
            {
                await _logger.LogTentativaVerificacao(
                    $"{clienteAtual.CliNome} ({clienteAtual.CliTelefoneCelular})",
                    codigoInformado,
                    false
                );

                return new VerificacaoCodigoResultado { Sucesso = false, Erro = "Código incorreto. Tente novamente." };
            }

            await _logger.LogTentativaVerificacao(
                $"{clienteAtual.CliNome} ({clienteAtual.CliTelefoneCelular})",
                codigoInformado,
                true
            );

            TabCliente clienteVerificado = clienteAtual;

            var resultado = await _cadastroCliente.ProcessarAsync(clienteVerificado);

            _sessaoVerificacao.Limpar();

            return new VerificacaoCodigoResultado
            {
                Sucesso = true,
                CadastroSucesso = resultado.CadastroSucesso,
                NomeCliente = clienteVerificado.CliNome,
                EmailSucesso = resultado.EmailSucesso
            };
        }

        public async Task<ReenvioResultado> ReenviarPorEmailAsync()
        {
            if (!_sessaoVerificacao.TryObter(out var dadosSessao))
            {
                return new ReenvioResultado { Sucesso = false, Mensagem = "Sessão expirada. Por favor, reinicie o processo." };
            }

            if (!_codigoVerificacao.EstaValido(dadosSessao.EmitidoEm))
            {
                return new ReenvioResultado { Sucesso = false, Mensagem = "Código expirado. Por favor, reinicie o processo." };
            }

            var cliente = dadosSessao.Cliente;
            var codigoVerificacao = dadosSessao.Codigo;

            bool enviado = await _apiClient.EnviarCodigo(
                        cliente.CliEmail,
                        cliente.CliNome,
                        codigoVerificacao
                    );

            if (enviado)
            {
                return new ReenvioResultado { Sucesso = true, Mensagem = "Código enviado por email com sucesso!" };
            }
            else
            {
                return new ReenvioResultado { Sucesso = false, Mensagem = "Erro ao enviar email. Tente novamente." };
            }
        }

        public async Task<ReenvioResultado> ReenviarPorWhatsAppAsync()
        {
            if (!_sessaoVerificacao.TryObter(out var dadosSessao))
            {
                return new ReenvioResultado { Sucesso = false, Mensagem = "Sessão expirada. Refaça o cadastro." };
            }

            var demonstracao = dadosSessao.Cliente;

            var novoCodigo = _codigoVerificacao.Gerar();

            _sessaoVerificacao.AtualizarCodigo(novoCodigo, DateTime.Now);

            await _logger.LogInicioProcesso(demonstracao.CliNome, demonstracao.CliTelefoneCelular, demonstracao.CliEmail);

            bool sucessoWhatsApp = false;
            string whatsappErro = null;

            try
            {
                string numeroDestino = _dadosService.NormalizarBrasil(demonstracao.CliTelefoneCelular);
                
                var resultadoEnvio = await _disparoMensagem.EnviarTemplateAsync(numeroDestino, _configuration["Digisac:CodeTemplateId"], true, novoCodigo);

                if (resultadoEnvio.Sucesso)
                {
                    sucessoWhatsApp = true;
                }
                else
                {
                    whatsappErro = $"Erro no disparo: {resultadoEnvio.Erro}";
                }
            }
            catch (Exception exWhatsApp)
            {
                whatsappErro = $"Erro ao enviar WhatsApp: {exWhatsApp.Message}";
            }

            await _logger.LogDisparoWhatsApp(demonstracao.CliTelefoneCelular, novoCodigo, sucessoWhatsApp, whatsappErro);

            if (sucessoWhatsApp)
            {
                return new ReenvioResultado
                {
                    Sucesso = true,
                    Mensagem = "Novo código enviado com sucesso!",
                    Telefone = demonstracao.CliTelefoneCelular
                };
            }
            else
            {
                return new ReenvioResultado { Sucesso = false, Mensagem = "Erro ao enviar código. Tente novamente." };
            }
        }

        public async Task<string> ValidarEmailAsync(string chave, string email)
        {
            UsuarioInfo usuarioInfo = null;

            TabCliente oCliente = await APIClient.Factory.Instance.GetClienteByEmail(HexadecimalEncoding.FromHexString(email));

            if (oCliente != null && oCliente.CliEmailVerified.GetValueOrDefault())
            {
                return $"<h2>Olá {oCliente.CliNome}</h2><br /><p>Seja bem vindo à TV Player.</p><p>Em breve você estará recebendo em seu email o procedimento para teste do nosso software.</p>";
            }

            usuarioInfo = await APIClient.Factory.Instance.VerificaEmailCliente($"2_{chave}_{HexadecimalEncoding.FromHexString(email)}");

            if (usuarioInfo != null)
            {
                if (!string.IsNullOrEmpty(usuarioInfo.UsuarioNome))
                {
                    bool emailEnviado = false;
                    string emailErro = null;
                    try
                    {
                        var resultadoEmail = _emailService.EnviarProcedimentosInstalacao(usuarioInfo);
                        emailEnviado = resultadoEmail.sucesso;
                        emailErro = resultadoEmail.erro;
                        if (!emailEnviado && string.IsNullOrEmpty(emailErro))
                            emailErro = "Falha no envio do email de procedimentos de instalação (erro desconhecido)";
                    }
                    catch (Exception ex)
                    {
                        emailEnviado = false;
                        emailErro = ex.Message;
                    }
                    await _logger.LogDisparoEmail(HexadecimalEncoding.FromHexString(email), "PROCEDIMENTOS_INSTALACAO", emailEnviado, emailErro);

                    bool whatsappSucesso = false;
                    string whatsappErro = null;

                    string numeroDestino = _dadosService.NormalizarBrasil(usuarioInfo.UsuarioCelular);
                    string nomeEmpresa = _dadosService.FormatarNomeEmpresa(oCliente.CliEmpresa);
                   

                    try
                    {
                        string nome = usuarioInfo.UsuarioNome;
                        var nomeCliente = _dadosService.FormatarNomePessoa(nome);
                        

                        if (!string.IsNullOrEmpty(numeroDestino))
                        {
                            var resultadoWhatsApp = await _disparoMensagem.EnviarTemplateAsync(numeroDestino, _configuration["Digisac:AccountSuccessTemplateId"], false, nomeCliente, nomeEmpresa);

                            whatsappSucesso = resultadoWhatsApp.Sucesso;
                            whatsappErro = resultadoWhatsApp.Erro;
                        }
                        else
                        {
                            whatsappSucesso = false;
                            whatsappErro = "Telefone não encontrado";
                        }
                    }
                    catch (Exception ex)
                    {
                        whatsappSucesso = false;
                        whatsappErro = ex.Message;
                    }
                    await _logger.LogDisparoWhatsApp(
                        usuarioInfo?.UsuarioCelular ?? "Não disponível",
                        "PROCEDIMENTOS_INSTALACAO",
                        whatsappSucesso,
                        whatsappErro
                    );

                    return $"<h2>Olá {usuarioInfo.UsuarioNome}</h2><br /><p>Seja bem vindo à TV Player.</p><p>Em breve você estará recebendo em seu email o procedimento para teste do nosso software.</p>";
                }

                return null;
            }

            return "<h2>Erro na validação</h2><br /><p>Link inválido ou expirado. Entre em contato conosco para mais informações.</p>";
        }
    }
}
