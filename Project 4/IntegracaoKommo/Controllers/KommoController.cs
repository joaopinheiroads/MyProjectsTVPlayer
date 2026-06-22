using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using TVPlayerSite.API.Interfaces.UnitOfWork;
using TVPlayerSite.API.DTO;
using System.Linq;
namespace TVPlayerSite.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class KommoController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<KommoController> _logger;
        private readonly IKommoService _kommoService;



        public KommoController(
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            ILogger<KommoController> logger,
            IKommoService kommoService)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _logger = logger;
            _kommoService = kommoService;
        }

        [AllowAnonymous]
        [HttpGet("mover-lead")]
        public async Task<IActionResult> MoverLead([FromQuery] string leadId, [FromQuery] string statusId)
        {
            if (string.IsNullOrEmpty(leadId) || string.IsNullOrEmpty(statusId))
                return BadRequest(new { error = "Informe leadId e statusId como query params." });

            try
            {
                var baseUrl = _configuration["Kommo:BaseUrl"];
                var token = _configuration["Kommo:AccessToken"];

                if (string.IsNullOrEmpty(baseUrl) || string.IsNullOrEmpty(token))
                    return StatusCode(500, new { error = "Kommo:BaseUrl ou Kommo:AccessToken não configurados no appsettings.json" });

                var httpClient = _httpClientFactory.CreateClient();
                httpClient.BaseAddress = new Uri(baseUrl);
                httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);

                
                var body = new[]
                {
                    new
                    {
                        id        = long.Parse(leadId),
                        status_id = long.Parse(statusId)
                    }
                };

                var json = JsonConvert.SerializeObject(body);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                _logger.LogInformation($"🚀 PATCH /api/v4/leads — lead {leadId} → status {statusId}");
                _logger.LogInformation($"Body: {json}");

                var response = await httpClient.PatchAsync("/api/v4/leads", content);
                var responseBody = await response.Content.ReadAsStringAsync();

                _logger.LogInformation($"Status HTTP: {(int)response.StatusCode}");
                _logger.LogInformation($"Resposta: {responseBody}");

                if (response.IsSuccessStatusCode)
                {
                    return Ok(new
                    {
                        message = "✅ Lead movido com sucesso!",
                        leadId,
                        statusId,
                        httpStatus = (int)response.StatusCode,
                        resposta = JsonConvert.DeserializeObject(responseBody)
                    });
                }

                return StatusCode((int)response.StatusCode, new
                {
                    error = "❌ Erro ao mover lead no Kommo",
                    httpStatus = (int)response.StatusCode,
                    resposta = JsonConvert.DeserializeObject(responseBody)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Erro inesperado ao mover lead");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [Authorize]
        [HttpPost("AtualizarGrupo")]
        public async Task<IActionResult> AtualizarGrupo([FromBody] GrupoAtualizadoDTO dto) {
            try {
                if (dto == null || dto.GrupoId == 0)
                    return BadRequest(new { error = "GrupoId inválido." });

                _logger.LogInformation($"📥 POST /api/AtualizarGrupo — GrupoId={dto.GrupoId}");

                await _kommoService.ProcessarAsync(dto);

                return Ok(new { message = "GrupoAtualizado processado com sucesso", grupoId = dto.GrupoId });
            }
            catch (Exception ex) {
                _logger.LogError(ex, "❌ Erro ao processar GrupoAtualizado");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [AllowAnonymous]
        [HttpPost("ReceiveWebHook")]
        public async Task<IActionResult> ReceiveWebhook() {
            try {
                _logger.LogInformation("========== WEBHOOK RECEBIDO ==========");
                _logger.LogInformation($"Timestamp: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");

                var form = await Request.ReadFormAsync();

                _logger.LogInformation("--- DADOS DO WEBHOOK ---");
                foreach (var field in form)
                    _logger.LogInformation($"{field.Key} = {field.Value}");

                var isContactAdd = form.Keys.Any(k => k.StartsWith("contacts[add]"));
                if (!isContactAdd) {
                    _logger.LogInformation("ℹ️ Evento ignorado — não é criação de contato.");
                    return Ok(new { message = "Evento ignorado" });
                }


                var linkedKey = form.Keys.FirstOrDefault(k =>
                    k.StartsWith("contacts[add][0][linked_leads_id][") && k.EndsWith("][ID]"));

                var leadId = form["leads[add][0][id]"].ToString();
                if (string.IsNullOrEmpty(leadId) && linkedKey != null) {
                    leadId = form[linkedKey].ToString();
                    _logger.LogInformation($"🔗 Lead ID extraído do linked_leads_id: {leadId}");
                }

                if (string.IsNullOrEmpty(leadId)) {
                    _logger.LogWarning("⚠️ Webhook recebido mas não contém ID de lead");
                    return Ok(new { message = "Webhook recebido mas sem ID de lead" });
                }


                var payload = new WebhookPayloadDTO {
                    LeadId = leadId,
                    ContactId = form["contacts[add][0][id]"].ToString(),
                    Nome = form["contacts[add][0][name]"].ToString(),
                    Telefone = form["contacts[add][0][custom_fields][0][values][0][value]"].ToString(),
                    Email = form["contacts[add][0][custom_fields][1][values][0][value]"].ToString()
                };

                _logger.LogInformation($"📋 Lead ID: {payload.LeadId}");
                _logger.LogInformation($"👤 Nome: {payload.Nome}");
                _logger.LogInformation($"📱 Telefone: {payload.Telefone}");
                _logger.LogInformation($"📧 Email: {payload.Email}");

                await _kommoService.ProcessarWebhookAsync(payload);

                _logger.LogInformation("✅ Webhook processado com sucesso!");
                return Ok(new { message = "Webhook processado", leadId });
            }
            catch (Exception ex) {
                _logger.LogError(ex, "❌ Erro ao processar webhook");
                return StatusCode(500, new { error = ex.Message });
            }
        }
        [Authorize]
        [HttpGet("Test")]
        public IActionResult Test() {
            var kommoConfigured = !string.IsNullOrEmpty(_configuration["Kommo:BaseUrl"]) &&
                                  !string.IsNullOrEmpty(_configuration["Kommo:AccessToken"]);

            return Ok(new {
                message = "Webhook endpoint está funcionando!",
                timestamp = DateTime.Now,
                url = $"{Request.Scheme}://{Request.Host}{Request.Path}",
                kommoConfigured,
                kommoBaseUrl = _configuration["Kommo:BaseUrl"] ?? "NÃO CONFIGURADO"
            });
        }
    }
}
