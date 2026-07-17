using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TVPlayerSite.Classes;
using TVPlayerSite.Helpers;
using TVPlayerSite.APIClient;
using DevExpress.Data.TreeList;
using TVPlayerSite.Models.CRM;

namespace TVPlayerSite.Controllers
{
    public class DemonstracaoController : BaseCadastroController
    {
        private static readonly string DemonstracaoTemp = "DemonstracaoTemp";
        private static readonly string CodigoVerificacaoTemp = "CodigoVerificacaoTemp";
        private static readonly string TimestampVerificacaoTemp = "TimestampVerificacaoTemp";
        private readonly ApiClient _apiClient;
        
       
        TabCliente _demonstracao;
        TabLicenca _licenca;

        public static string ObterSaudacao()
        {
            var horaAtual = DateTime.Now.Hour;

            if (horaAtual >= 5 && horaAtual < 12)
                return "bom dia";
            else if (horaAtual >= 12 && horaAtual < 18)
                return "boa tarde";
            else
                return "boa noite";
        }
        string saudacao = ObterSaudacao();

        // Método para gerar código de verificação de 4 dígitos
        private string GerarCodigoVerificacao()
        {
            Random random = new Random();
            return random.Next(1000, 9999).ToString();
        }

        // Método para verificar se o código ainda é válido (5 minutos)
        private bool CodigoEstaValido(DateTime timestamp)
        {
            return DateTime.Now.Subtract(timestamp).TotalMinutes <= 5;
        }

       

        public DemonstracaoController(
            IHostingEnvironment env, IStringLocalizer<ResourceCommon> localizerCommon, ApiClient apiClient)
            : base(env, localizerCommon)
        {
            _apiClient = apiClient;
            
        }

        public void ViewDataItems()
        {
            LoadPageContent("demonstracao", allowAMP: false);

            ViewData["Title"] = "Demonstração TV Player";
            ViewData["TextoH1"] = "TV Player - ";
            ViewData["TextoH1Strong"] = "Avalie nosso Software";
        }

        [Route("{lang}/demonstracao")]
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            ViewDataItems();
            await CarregarEstados();

            string sessaoCliente = HttpContext.Session.GetString(DemonstracaoTemp);
            if (!string.IsNullOrEmpty(sessaoCliente))
                _demonstracao = JsonConvert.DeserializeObject<TabCliente>(sessaoCliente);
            return View(_demonstracao);
        }

        [HttpPost]
        public async Task<IActionResult> Index(TabCliente cliente)
        {
            ViewDataItems();

            if (cliente != null)
            {
                try
                {
                    if (string.IsNullOrEmpty(cliente.ReCaptcha))
                        ModelState.AddModelError("ReCaptcha", _localizerCommon["verificação obrigatória"]);

                    if (ModelState.IsValid)
                    {
                        // Log do início do processo
                        await LogVerificacaoHelper.LogInicioProcesso(cliente.CliNome, cliente.CliTelefoneCelular, cliente.CliEmail);
                        
                        // Gerar código de verificação
                        string codigoVerificacao = GerarCodigoVerificacao();
                        
                        // Salvar dados temporariamente na sessão
                        HttpContext.Session.SetString(DemonstracaoTemp, JsonConvert.SerializeObject(cliente));
                        HttpContext.Session.SetString(CodigoVerificacaoTemp, codigoVerificacao);
                        HttpContext.Session.SetString(TimestampVerificacaoTemp, DateTime.Now.ToString());

                        // Enviar código via WhatsApp
                        bool whatsappSucesso = false;
                        string whatsappErro = null;
                        
                        try
                        {
                            string nomeCliente = (cliente.CliNome ?? "").Trim();
                            string numeroDestino = Regex.Replace(cliente.CliTelefoneCelular ?? "", "[^0-9]", "");

                            string mensagem = $"Olá *{nomeCliente}*, {saudacao}!\n\n" +
                                             $"Seu código de verificação da TV Player é: *{codigoVerificacao}*\n\n" +
                                             "Este código é válido por 5 minutos.\n\n" +
                                             "Digite o código na tela para finalizar seu cadastro de demonstração.";

                            // Usar DisparosChat diretamente (igual ao VerificadorDeDemonstracoes)
                            var disparosChat = new TVPlayerSite.Disparos.DisparosChat();
                            var resultadoEnvio = await disparosChat.EnviarMensagemAsync(numeroDestino, mensagem);
                            bool enviado = await _apiClient.EnviarCodigo(
                            cliente.CliEmail,
                            cliente.CliNome,
                            codigoVerificacao
                        );
                            if (resultadoEnvio.Sucesso)
                            {
                                whatsappSucesso = true;
                                Console.WriteLine($"Código {codigoVerificacao} enviado com sucesso para: {cliente.CliNome}, Telefone: {numeroDestino}");
                            }
                            else
                            {
                                whatsappSucesso = false;
                                whatsappErro = resultadoEnvio.Erro ?? "Falha no envio via WhatsApp";
                                Console.WriteLine($"Falha ao enviar código para {cliente.CliNome}: {whatsappErro}");
                            }
                        }
                        catch (Exception ex)
                        {
                            whatsappSucesso = false;
                            whatsappErro = ex.Message;
                            Console.WriteLine($"Erro ao enviar código: {ex.Message}");
                            // Continua mesmo se houver erro no envio do WhatsApp
                        }
                        
                        // Log do disparo WhatsApp
                        await LogVerificacaoHelper.LogDisparoWhatsApp(cliente.CliTelefoneCelular, codigoVerificacao, whatsappSucesso, whatsappErro);

                        // Diagnóstico - verificar se é AJAX
                        var isAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest";
                        Console.WriteLine($"[DIAGNÓSTICO] É AJAX: {isAjax}");
                        Console.WriteLine($"[DIAGNÓSTICO] Headers: {string.Join(", ", Request.Headers.Select(h => $"{h.Key}:{h.Value}"))}");

                        // Melhorar detecção AJAX - verificar múltiplos indicadores
                        var isAjaxRequest = Request.Headers["X-Requested-With"] == "XMLHttpRequest" || 
                                          Request.Headers.ContainsKey("X-Requested-With") ||
                                          Request.ContentType?.Contains("application/json") == true ||
                                          Request.Query.ContainsKey("ajax") ||
                                          Request.Form.ContainsKey("ajax");

                        Console.WriteLine($"[DIAGNÓSTICO] Forçando retorno JSON para always usar modal");

                        // SEMPRE retornar JSON para usar o modal - correção crítica
                        // Se não for AJAX, significa que algo está errado na detecção
                        return Json(new { 
                            sucesso = true, 
                            nomeCliente = cliente.CliNome,
                            telefone = cliente.CliTelefoneCelular,
                            whatsappSucesso = whatsappSucesso,
                            whatsappErro = whatsappErro,
                            debug = new {
                                isAjax = isAjaxRequest,
                                headers = Request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString())
                            }
                        });

                        // COMENTADO: Não usar mais a view separada
                        // Se não for AJAX, retornar view normal (backward compatibility)
                        // return View("VerificacaoCodigo", cliente);
                    }

                    // Se ModelState não é válido - SEMPRE retornar JSON
                    var erros = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    return Json(new { sucesso = false, erros = erros });

                    // COMENTADO: Não usar mais a view separada  
                    // return View(cliente);
                }
                catch (Exception ex)
                {
                    // SEMPRE retornar JSON em caso de erro
                    return Json(new { sucesso = false, erro = "Erro interno. Tente novamente." });
                    
                    // COMENTADO: Não redirecionar mais para ErrorPage
                    // return RedirectToAction(ErrorPage);
                }
            }

            return View(null);
        }

      


        [Route("{lang}/demonstracao/verificar-codigo-ajax")]
        [HttpPost]
        public async Task<IActionResult> VerificarCodigoAjax(string codigoInformado)
        {
            try
            {
                // Recuperar dados da sessão
                string codigoCorreto = HttpContext.Session.GetString(CodigoVerificacaoTemp);
                string timestampStr = HttpContext.Session.GetString(TimestampVerificacaoTemp);
                string clienteJson = HttpContext.Session.GetString(DemonstracaoTemp);

                if (string.IsNullOrEmpty(codigoCorreto) || string.IsNullOrEmpty(timestampStr) || string.IsNullOrEmpty(clienteJson))
                {
                    return Json(new { sucesso = false, erro = "Sessão expirada. Por favor, faça o cadastro novamente." });
                }

                DateTime timestamp = DateTime.Parse(timestampStr);

                if (!CodigoEstaValido(timestamp))
                {
                    // Log de expiração de código
                    await LogVerificacaoHelper.LogExpiracaoTempo(clienteJson);

                    return Json(new { sucesso = false, erro = "Código expirado. Por favor, faça o cadastro novamente.", expirado = true });
                }

                TabCliente clienteAtual = JsonConvert.DeserializeObject<TabCliente>(clienteJson);

                

                if (codigoInformado != codigoCorreto)
                {
                    // Log de tentativa de verificação incorreta
                    await LogVerificacaoHelper.LogTentativaVerificacao(
                        $"{clienteAtual.CliNome} ({clienteAtual.CliTelefoneCelular})",
                        codigoInformado,
                        false
                    );

                    return Json(new { sucesso = false, erro = "Código incorreto. Tente novamente." });
                }

                // Log de verificação bem-sucedida
                await LogVerificacaoHelper.LogTentativaVerificacao(
                    $"{clienteAtual.CliNome} ({clienteAtual.CliTelefoneCelular})",
                    codigoInformado,
                    true
                );

                // Código correto - processar cadastro
                TabCliente clienteVerificado = JsonConvert.DeserializeObject<TabCliente>(clienteJson);

               

                clienteVerificado.CliTipoId = 4;
                string chave = Guid.NewGuid().ToString();
                Console.WriteLine($"[EMAIL_CONFIRMACAO_AJAX] Chave gerada no controller: {chave}");

                _licenca = new TabLicenca
                {
                    LicChave = chave,
                    LicValidade = 96,
                    LicDataCadastro = DateTime.Now,
                    LicUnidadeId = 1,
                    LicUsuarioIdcadastro = 1,
                    LicTipoDeLicencaId = 5
                };

                clienteVerificado.TabLicenca.Add(_licenca);
                Console.WriteLine($"[EMAIL_CONFIRMACAO_AJAX] Licença adicionada ao cliente com chave: {_licenca.LicChave}");

                // Verificar se usuário já existe ANTES de tentar salvar
                Usuario usuarioExistente = null;
                
                


                try
                {
                    usuarioExistente = await _apiClient.GetUsuario(clienteVerificado.CliEmail);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[VERIFICACAO_USUARIO] Não foi possível verificar usuário existente: {ex.Message}");
                    // Continua assumindo que é um usuário novo se não conseguir verificar
                }
                
                bool result;
                bool emailSucesso = false;
                string emailErro = null;

                if (usuarioExistente != null)
                {
                    // Cliente já existe - enviar email de cadastro existente
                    Console.WriteLine($"[EMAIL_CONFIRMACAO_AJAX] Cliente já existe: {clienteVerificado.CliEmail}");
                    try
                    {
                        var emailHelper = new EmailHelper();
                        var resultadoEmail = emailHelper.EnviarEmailDemonstracaoExistenteComDetalhes($"{_hostingEnvironment.WebRootPath}/email/txt/cadastroExistente.txt", usuarioExistente);
                        result = resultadoEmail.sucesso;
                        emailSucesso = resultadoEmail.sucesso;
                        emailErro = resultadoEmail.erro;




                        int idUsuario = usuarioExistente.ID;

                        var link = $"https://app.tvplayer.com.br:44909/digitalsignage/sadm/pages/solicitacaocontratacao/frmcontratacao.aspx?tvp=" + EncryptHelper.Encrypt(idUsuario.ToString());

                        string nomeEmpresa = (clienteVerificado.CliEmpresa ?? "").Trim();
                        var Mensagem = $"Olá, *{clienteVerificado.CliNome}*, empresa *{nomeEmpresa}*!\n\n" +
                                         "Encontramos seu e-mail cadastrado em nosso sistema.\n\n" +
                                         "Entre em *contato* com nosso atendende via *WhatsApp* (Opção 4 - Falar com atendente)\n\n" +
                                         $"Caso queira estar efetivando a contratação, *clique aqui* {link}\n\n";


                        string numeroDestino = Regex.Replace(clienteVerificado.CliTelefoneCelular ?? "", "[^0-9]", "");

                        var disparosChat = new TVPlayerSite.Disparos.DisparosChat();
                        await disparosChat.EnviarMensagemAsync(numeroDestino, Mensagem);




                        if (!result && string.IsNullOrEmpty(emailErro)) 
                            emailErro = "Falha no envio de email para usuário existente (erro desconhecido)";


                    }
                    catch (Exception ex)
                    {
                        result = false;
                        emailSucesso = false;
                        emailErro = ex.Message;
                    }
                }
                else
                {
                    // Cliente novo - salvar e enviar email de confirmação
                    Console.WriteLine($"[EMAIL_CONFIRMACAO_AJAX] Cliente novo - salvando: {clienteVerificado.CliEmail}");
                    var newDemonstracao = await _apiClient.SaveCliente(clienteVerificado);



                    if (newDemonstracao != null)
                    {

                        var emailHelper = new EmailHelper();

                        // Monta o link
                        var licenca = clienteVerificado.TabLicenca.First();
                        string linkConfirmacao = $"https://www.tvplayer.com.br/pt/demonstracao/validaremail" +
                                                 $"?p={licenca.LicChave}" +
                                                 $"&e={HexadecimalEncoding.ToHexString(clienteVerificado.CliEmail)}";
                        try
                        {
                            Console.WriteLine($"[EMAIL_CONFIRMACAO_AJAX] Enviando email com chave: {clienteVerificado.TabLicenca.FirstOrDefault()?.LicChave}");
                            
                            var resultadoEmail = emailHelper.EnviarEmailSolicitacaoDemonstracaoComDetalhes($"{_hostingEnvironment.WebRootPath}/email/txt/confirmarEmail.txt", clienteVerificado);

                           

                            result = resultadoEmail.sucesso;
                            emailSucesso = resultadoEmail.sucesso;
                            emailErro = resultadoEmail.erro;
                            
                            if (!result && string.IsNullOrEmpty(emailErro)) 
                                emailErro = "Falha no envio de email de confirmação (erro desconhecido)";

                            var resultadoEmailDois = resultadoEmail;

                        }
                        catch (Exception ex)
                        {
                            result = false;
                            emailSucesso = false;
                            emailErro = ex.Message;
                        }


                        // Enviar mensagem de boas-vindas via WhatsApp (só para clientes novos salvos com sucesso)
                        bool whatsappBoasVindasSucesso = false;
                        string whatsappBoasVindasErro = null;




                        var nomeClienteDois = clienteVerificado.CliNome;
                        var numeroDestinoDois = Regex.Replace(clienteVerificado.CliTelefoneCelular ?? "", "[^0-9]", "");

                        try {
                            string mensagemDois = $"*Olá, {nomeClienteDois}.*\n\n" +
                                       "Obrigado por escolher a TV Player!\n\n" +
                                       "*Confirme seu e-mail clicando no link abaixo:* \n\n" +
                                       $"*{linkConfirmacao}*\n";

                            var disparosChat = new TVPlayerSite.Disparos.DisparosChat();
                            var resultadoMensagemDois = await disparosChat.EnviarMensagemAsync(numeroDestinoDois, mensagemDois);

                            Console.WriteLine(resultadoMensagemDois.Sucesso
                                ? $"[WHATSAPP_CONFIRMACAO] Enviado com sucesso para {nomeClienteDois}"
                                : $"[WHATSAPP_CONFIRMACAO] Falha: {resultadoMensagemDois.Erro}");

                            if(resultadoMensagemDois.Sucesso) {
                                whatsappBoasVindasSucesso = true;
                            } else {
                                whatsappBoasVindasSucesso = false;
                                whatsappBoasVindasErro = resultadoMensagemDois.Erro ?? "Falha no envio do WhatsApp de boas-vindas (erro desconhecido)";
                            }
                            // Log do disparo WhatsApp de boas-vindas
                            await LogVerificacaoHelper.LogDisparoWhatsApp(
                                clienteVerificado.CliTelefoneCelular,
                                "BOAS_VINDAS",
                                whatsappBoasVindasSucesso,
                                whatsappBoasVindasErro
                            );

                        }
                        catch (Exception ex) {
                            whatsappBoasVindasSucesso = false;
                            whatsappBoasVindasErro = ex.Message;

                            Console.WriteLine($"[WHATSAPP_CONFIRMACAO] Erro: {ex.Message}");
                        }

                      

                        // Log do disparo WhatsApp de boas-vindas
                        await LogVerificacaoHelper.LogDisparoWhatsApp(
                            clienteVerificado.CliTelefoneCelular,
                            "BOAS_VINDAS",
                            whatsappBoasVindasSucesso,
                            whatsappBoasVindasErro
                        );

                        // Log do disparo de email
                        await LogVerificacaoHelper.LogDisparoEmail(
                            clienteVerificado.CliEmail,
                            "CONFIRMACAO",
                            emailSucesso,
                            emailErro
                        );

                        // Log do cadastro completo
                        if (result)
                        {
                            await LogVerificacaoHelper.LogSucessoCadastro(
                                clienteVerificado.CliNome,
                                clienteVerificado.CliTelefoneCelular,
                                clienteVerificado.CliEmail
                            );
                        }
                    }
                    else
                    {
                        result = false;
                        emailSucesso = false;
                        emailErro = "Falha ao salvar cliente";

                        // Log do erro de salvamento
                        await LogVerificacaoHelper.LogDisparoEmail(
                            clienteVerificado.CliEmail,
                            "CONFIRMACAO",
                            false,
                            emailErro
                        );
                    }
                }

                // Limpar dados da sessão
                HttpContext.Session.Remove(DemonstracaoTemp);
                HttpContext.Session.Remove(CodigoVerificacaoTemp);
                HttpContext.Session.Remove(TimestampVerificacaoTemp);

                return Json(new
                {
                    sucesso = true,
                    cadastroSucesso = result,
                    nomeCliente = clienteVerificado.CliNome,
                    emailSucesso = emailSucesso
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro na verificação AJAX: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                
                // Em desenvolvimento, retornar erro detalhado
                var errorMessage = $"Erro interno: {ex.Message}";
                if (ex.InnerException != null)
                {
                    errorMessage += $" | Inner: {ex.InnerException.Message}";
                }
                
                return Json(new { sucesso = false, erro = errorMessage });
            }


            
        }   
      

        public ActionResult Success()
        {
            ViewDataItems();

            // Para a página de sucesso, não precisamos dos dados do cliente
            // pois o cadastro já foi processado com sucesso
            return View();
        }

        public ActionResult Error()
        {
            ViewDataItems();

            // Para a página de erro, também não precisamos dos dados específicos
            return View();
        }

        public async Task<ActionResult> ValidarEmail([FromQuery(Name = "p")] string chave, [FromQuery(Name = "e")] string email)
        {
            ViewDataItems();

            UsuarioInfo usuarioInfo = null;
            
            try
            {
                TabCliente oCliente = await APIClient.Factory.Instance.GetClienteByEmail(HexadecimalEncoding.FromHexString(email));

                if (oCliente != null && oCliente.CliEmailVerified.GetValueOrDefault()) {

                    ViewData["ValidationMessage"] = $"<h2>Olá {oCliente.CliNome}</h2><br /><p>Seja bem vindo à TV Player.</p><p>Em breve você estará recebendo em seu email o procedimento para teste do nosso software.</p>";
                    return View();
                }

                // Buscar dados do usuário (senha)
                usuarioInfo = await APIClient.Factory.Instance.VerificaEmailCliente($"2_{chave}_{HexadecimalEncoding.FromHexString(email)}");

            



            }
            catch (Exception ex)
            {
                throw ex;
            }

            if (usuarioInfo != null)
            {
                if (!string.IsNullOrEmpty(usuarioInfo.UsuarioNome))
                {
                    // Enviar email de procedimentos (mantém igual)
                    bool emailEnviado = false;
                    string emailErro = null;
                    try
                    {
                        var emailHelper = new EmailHelper();
                        var resultadoEmail = emailHelper.EnviarEmailProcedimentoInstalacaoComDetalhes($"{_hostingEnvironment.WebRootPath}/email/txt/procedimentos.txt", usuarioInfo);
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
                    await LogVerificacaoHelper.LogDisparoEmail(HexadecimalEncoding.FromHexString(email), "PROCEDIMENTOS_INSTALACAO", emailEnviado, emailErro);

                    // Enviar WhatsApp SEMPRE
                    bool whatsappSucesso = false;
                    string whatsappErro = null;

                    //TVPlayerSite.Models.Video.Usuario clienteCompleto = await _apiClient.GetUsuarioTerminalByEmailASync($"{HexadecimalEncoding.FromHexString(email)}");
                    string numeroDestino = usuarioInfo.UsuarioCelular?.Trim();
                    string chaveAtivacao = usuarioInfo.TerminalCodInventario;

                    try
                    {  
                        string nomeCliente = usuarioInfo.UsuarioNome?.Trim().ToUpper() ?? "CLIENTE";
                        //string numeroDestino = clienteCompleto != null ? Regex.Replace(clienteCompleto.CliTelefoneCelular ?? "", "[^0-9]", "") : "";
                        string emailCliente = HexadecimalEncoding.FromHexString(email);
                        string senhaCliente = usuarioInfo.UsuarioSenha ?? "Não disponível";

                        string mensagem = $"*OLÁ {nomeCliente}.*\n\n" +
                            "O procedimento de instalação e ativação do player estarão disponíveis após efetuar o login no painel administrativo.\n\n" +
                            "*Para acessar o painel administrativo, utilize o link:* www.tvplayer.com.br\n\n" +
                            "Clique no botão da parte superior direita: '*Acessar o Sistema*'.\n\n" +
                            $"*Seu login e senha para acesso são:*\n" +
                            $"Login: {emailCliente}\n" +
                            $"Senha: {senhaCliente}\n\n" +
                            $"*Sua chave de ativação do Player é: {chaveAtivacao}*\n\n" +
                            "Após a ativação, no menu lateral do *painel de gerenciamento*, entrar em *Ajuda -- Tutoriais*, lá você encontrará Vídeos de como utilizar o sistema.\n\n" +
                            "Obs: os dados acima foram enviados em cópia para o e-mail cadastrado.\n\n"+
                            "Atenciosamente,\n" +
                            "Equipe TV Player";

                        if (!string.IsNullOrEmpty(numeroDestino))
                        {
                            
                            var disparosChat = new TVPlayerSite.Disparos.DisparosChat();
                            var resultadoWhatsApp = await disparosChat.EnviarMensagemAsync(numeroDestino, mensagem);

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
                    await LogVerificacaoHelper.LogDisparoWhatsApp(
                        usuarioInfo?.UsuarioCelular ?? "Não disponível",
                        "PROCEDIMENTOS_INSTALACAO",
                        whatsappSucesso,
                        whatsappErro
                    );

                    ViewData["ValidationMessage"] = $"<h2>Olá {usuarioInfo.UsuarioNome}</h2><br /><p>Seja bem vindo à TV Player.</p><p>Em breve você estará recebendo em seu email o procedimento para teste do nosso software.</p>";
                }
            }
            else
            {
                ViewData["ValidationMessage"] = "<h2>Erro na validação</h2><br /><p>Link inválido ou expirado. Entre em contato conosco para mais informações.</p>";
            }

            return View();
        }

        [Route("{lang}/demonstracao/reenviar-codigo-email")]
        [HttpPost]
        public async Task<IActionResult> ReenviarCodigoPorEmail()
        {
            try
            {
                // Recuperar dados da sessão
                var clienteJson = HttpContext.Session.GetString(DemonstracaoTemp);
                var codigoVerificacao = HttpContext.Session.GetString(CodigoVerificacaoTemp);
                var timestampStr = HttpContext.Session.GetString(TimestampVerificacaoTemp);

                if (string.IsNullOrEmpty(clienteJson) || string.IsNullOrEmpty(codigoVerificacao) || string.IsNullOrEmpty(timestampStr))
                {
                    return Json(new { sucesso = false, mensagem = "Sessão expirada. Por favor, reinicie o processo." });
                }

                // Verificar se o código ainda é válido
                if (DateTime.TryParse(timestampStr, out DateTime timestamp) && !CodigoEstaValido(timestamp))
                {
                    return Json(new { sucesso = false, mensagem = "Código expirado. Por favor, reinicie o processo." });
                }

                var cliente = JsonConvert.DeserializeObject<TabCliente>(clienteJson);

                // Enviar código por email (já inclui o log)
               
                bool enviado = await _apiClient.EnviarCodigo(
                            cliente.CliEmail,
                            cliente.CliNome,
                            codigoVerificacao
                        );

                if (enviado)
                {
                    return Json(new { sucesso = true, mensagem = "Código enviado por email com sucesso!" });
                }
                else
                {
                    return Json(new { sucesso = false, mensagem = "Erro ao enviar email. Tente novamente." });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao reenviar código por email: {ex.Message}");
                
                // Log de erro na tentativa de reenvio
                try
                {
                    var clienteJson = HttpContext.Session.GetString(DemonstracaoTemp);
                    if (!string.IsNullOrEmpty(clienteJson))
                    {
                        var cliente = JsonConvert.DeserializeObject<TabCliente>(clienteJson);
                        await LogVerificacaoHelper.LogDisparoEmail(
                            cliente.CliEmail, 
                            "REENVIO_CODIGO", 
                            false, 
                            ex.Message
                        );
                    }
                }
                catch { }
                
                return Json(new { sucesso = false, mensagem = "Erro interno. Tente novamente." });
            }
        }

        [Route("{lang}/demonstracao/reenviar-codigo")]
        [HttpPost]
        public async Task<IActionResult> ReenviarCodigo()
        {
            try
            {
                Console.WriteLine("[REENVIO] Iniciando reenvio de código...");
                
                // Recuperar dados da demonstração da sessão
                var demonstracaoJson = HttpContext.Session.GetString(DemonstracaoTemp);
                Console.WriteLine($"[REENVIO] DemonstracaoJson recuperado: {(!string.IsNullOrEmpty(demonstracaoJson) ? "SIM" : "NÃO")}");
                
                if (string.IsNullOrEmpty(demonstracaoJson))
                {
                    Console.WriteLine("[REENVIO] ERRO: Sessão expirada");
                    return Json(new { sucesso = false, mensagem = "Sessão expirada. Refaça o cadastro." });
                }

                var demonstracao = JsonConvert.DeserializeObject<TabCliente>(demonstracaoJson);
                Console.WriteLine($"[REENVIO] Cliente: {demonstracao?.CliNome}, Telefone: {demonstracao?.CliTelefoneCelular}");

                // Gerar novo código de verificação
                var novoCodigo = GerarCodigoVerificacao();
                Console.WriteLine($"[REENVIO] Novo código gerado: {novoCodigo}");
                
                // Atualizar código e timestamp na sessão
                HttpContext.Session.SetString(CodigoVerificacaoTemp, novoCodigo);
                HttpContext.Session.SetString(TimestampVerificacaoTemp, DateTime.Now.ToString());
                Console.WriteLine("[REENVIO] Sessão atualizada");

                // Log do reenvio
                try
                {
                    await LogVerificacaoHelper.LogInicioProcesso(demonstracao.CliNome, demonstracao.CliTelefoneCelular, demonstracao.CliEmail);
                    Console.WriteLine("[REENVIO] Log inicial registrado");
                }
                catch (Exception exLog)
                {
                    Console.WriteLine($"[REENVIO] Erro no log inicial: {exLog.Message}");
                }

                // Enviar novo código via WhatsApp (baseado no padrão existente)
                bool sucessoWhatsApp = false;
                string whatsappErro = null;
                
                try
                {
                    string numeroDestino = Regex.Replace(demonstracao.CliTelefoneCelular ?? "", "[^0-9]", "");
                    string mensagem = $"Seu novo código de verificação TVPlayer é: *{novoCodigo}*";
                    Console.WriteLine($"[REENVIO] Preparando envio para: {numeroDestino}");
                    
                    // Usar DisparosChat diretamente (igual ao código existente)
                    var disparosChat = new TVPlayerSite.Disparos.DisparosChat();
                    var resultadoEnvio = await disparosChat.EnviarMensagemAsync(numeroDestino, mensagem);
                    Console.WriteLine($"[REENVIO] Resultado DisparosChat: {resultadoEnvio?.Sucesso}");
                    
                    if (resultadoEnvio.Sucesso)
                    {
                        sucessoWhatsApp = true;
                        Console.WriteLine($"Novo código {novoCodigo} enviado com sucesso para: {demonstracao.CliNome}, Telefone: {numeroDestino}");
                    }
                    else
                    {
                        whatsappErro = $"Erro DisparosChat: {resultadoEnvio.Erro}";
                        Console.WriteLine($"[ERRO REENVIO WHATSAPP] {whatsappErro}");
                    }
                }
                catch (Exception exWhatsApp)
                {
                    whatsappErro = $"Erro ao enviar WhatsApp: {exWhatsApp.Message}";
                    Console.WriteLine($"[ERRO REENVIO WHATSAPP] {whatsappErro}");
                }

                // Log do disparo WhatsApp
                try
                {
                    await LogVerificacaoHelper.LogDisparoWhatsApp(demonstracao.CliTelefoneCelular, novoCodigo, sucessoWhatsApp, whatsappErro);
                    Console.WriteLine("[REENVIO] Log disparo WhatsApp registrado");
                }
                catch (Exception exLog)
                {
                    Console.WriteLine($"[REENVIO] Erro no log WhatsApp: {exLog.Message}");
                }

                if (sucessoWhatsApp)
                {
                    Console.WriteLine("[REENVIO] Retornando sucesso");
                    return Json(new 
                    { 
                        sucesso = true, 
                        mensagem = "Novo código enviado com sucesso!",
                        telefone = demonstracao.CliTelefoneCelular
                    });
                }
                else
                {
                    Console.WriteLine("[REENVIO] Retornando erro de envio");
                    return Json(new { sucesso = false, mensagem = "Erro ao enviar código. Tente novamente." });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERRO REENVIO CODIGO] Exception: {ex.Message}");
                Console.WriteLine($"[ERRO REENVIO CODIGO] StackTrace: {ex.StackTrace}");
                return Json(new { sucesso = false, mensagem = $"Erro interno: {ex.Message}" });
            }
        }
    }
}
