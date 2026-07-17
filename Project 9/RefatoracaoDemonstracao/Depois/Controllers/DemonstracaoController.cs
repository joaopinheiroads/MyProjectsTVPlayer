using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System;
using System.Linq;
using System.Threading.Tasks;
using TVPlayerSite.Models.CRM;
using TVPlayerSite.Services.Demonstracao;

namespace TVPlayerSite.Controllers
{

    public class DemonstracaoController : BaseCadastroController
    {
        private readonly IDemonstracaoAppService _appService;
        private readonly IVerificacaoSessionStore _sessaoVerificacao;

        public DemonstracaoController(
            IHostingEnvironment env,
            IStringLocalizer<ResourceCommon> localizerCommon,
            IDemonstracaoAppService appService,
            IVerificacaoSessionStore sessaoVerificacao)
            : base(env, localizerCommon)
        {
            _appService = appService;
            _sessaoVerificacao = sessaoVerificacao;
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

            TabCliente demonstracao = null;
            if (_sessaoVerificacao.TryObter(out var dadosSessao))
                demonstracao = dadosSessao.Cliente;
            return View(demonstracao);
        }

        [HttpPost]
        public async Task<IActionResult> Index(TabCliente cliente)
        {
            ViewDataItems();

            if (cliente == null)
                return View(null);

            try
            {
                if (string.IsNullOrEmpty(cliente.ReCaptcha))
                    ModelState.AddModelError("ReCaptcha", _localizerCommon["verificação obrigatória"]);

                if (!ModelState.IsValid)
                {
                    var erros = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    return Json(new { sucesso = false, erros = erros });
                }

                var resultado = await _appService.IniciarVerificacaoAsync(cliente);

                var isAjaxRequest = Request.Headers["X-Requested-With"] == "XMLHttpRequest" ||
                                  Request.Headers.ContainsKey("X-Requested-With") ||
                                  Request.ContentType?.Contains("application/json") == true ||
                                  Request.Query.ContainsKey("ajax") ||
                                  Request.Form.ContainsKey("ajax");

                return Json(new
                {
                    sucesso = true,
                    nomeCliente = resultado.NomeCliente,
                    telefone = resultado.Telefone,
                    whatsappSucesso = resultado.WhatsappSucesso,
                    whatsappErro = resultado.WhatsappErro,
                    debug = new
                    {
                        isAjax = isAjaxRequest,
                        headers = Request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString())
                    }
                });
            }
            catch (Exception)
            {
                return Json(new { sucesso = false, erro = "Erro interno. Tente novamente." });
            }
        }

        [Route("{lang}/demonstracao/verificar-codigo-ajax")]
        [HttpPost]
        public async Task<IActionResult> VerificarCodigoAjax(string codigoInformado)
        {
            try
            {
                var resultado = await _appService.VerificarCodigoAsync(codigoInformado);

                if (!resultado.Sucesso)
                {
                    if (resultado.Expirado)
                        return Json(new { sucesso = false, erro = resultado.Erro, expirado = true });

                    return Json(new { sucesso = false, erro = resultado.Erro });
                }

                return Json(new
                {
                    sucesso = true,
                    cadastroSucesso = resultado.CadastroSucesso,
                    nomeCliente = resultado.NomeCliente,
                    emailSucesso = resultado.EmailSucesso
                });
            }
            catch (Exception ex)
            {
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

            return View();
        }

        public ActionResult Error()
        {
            ViewDataItems();

            return View();
        }

        public async Task<ActionResult> ValidarEmail([FromQuery(Name = "p")] string chave, [FromQuery(Name = "e")] string email)
        {
            ViewDataItems();

            ViewData["ValidationMessage"] = await _appService.ValidarEmailAsync(chave, email);

            return View();
        }

        [Route("{lang}/demonstracao/reenviar-codigo-email")]
        [HttpPost]
        public async Task<IActionResult> ReenviarCodigoPorEmail()
        {
            try
            {
                var resultado = await _appService.ReenviarPorEmailAsync();
                return Json(new { sucesso = resultado.Sucesso, mensagem = resultado.Mensagem });
            }
            catch (Exception)
            {
                return Json(new { sucesso = false, mensagem = "Erro interno. Tente novamente." });
            }
        }

        [Route("{lang}/demonstracao/reenviar-codigo")]
        [HttpPost]
        public async Task<IActionResult> ReenviarCodigo()
        {
            try
            {
                var resultado = await _appService.ReenviarPorWhatsAppAsync();

                if (resultado.Sucesso)
                {
                    return Json(new
                    {
                        sucesso = true,
                        mensagem = resultado.Mensagem,
                        telefone = resultado.Telefone
                    });
                }

                return Json(new { sucesso = false, mensagem = resultado.Mensagem });
            }
            catch (Exception ex)
            {
                return Json(new { sucesso = false, mensagem = $"Erro interno: {ex.Message}" });
            }
        }
    }
}
