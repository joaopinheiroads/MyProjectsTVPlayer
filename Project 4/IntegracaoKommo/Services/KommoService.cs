using System;

using System.Threading.Tasks;
using TVPlayerSite.API.Interfaces.UnitOfWork;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using TVPlayerSite.API.DTO;
using Microsoft.EntityFrameworkCore;
using TVPlayer.CRUD.Models;
using TVPlayerSite.Models.Video;








namespace TVPlayerSite.API.Services {
    public class KommoService : IKommoService {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<KommoService> _logger;
        private readonly VideoContext _context;
        private readonly ILeadRepository _leadRepository;

        private const long STATUS_CLIENTE = 142;
        private const long STATUS_TESTANDO = 89838888;

        public KommoService(
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            ILogger<KommoService> logger,
            VideoContext context,
            ILeadRepository leadRepository
            ) {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _logger = logger;
            _context = context;
            _leadRepository = leadRepository;
        }

        public async Task MoverLeadAsync(string leadId, long statusId) {
            try {
                var baseUrl = _configuration["Kommo:BaseUrl"];
                var token = _configuration["Kommo:AccessToken"];

                var httpClient = _httpClientFactory.CreateClient();
                httpClient.BaseAddress = new Uri(baseUrl);
                httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);

                var body = new[]
                {
                    new
                    {
                        id        = long.Parse(leadId),
                        status_id = statusId
                    }
                };

                var json = JsonConvert.SerializeObject(body);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                _logger.LogInformation($"🚀 PATCH /api/v4/leads — lead {leadId} → statusId {statusId}");

                var response = await httpClient.PatchAsync("/api/v4/leads", content);
                var responseBody = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                    _logger.LogInformation($"✅ Lead {leadId} movido com sucesso!");
                else
                    _logger.LogError($"❌ Erro ao mover lead {leadId}: {response.StatusCode} - {responseBody}");
            }
            catch (Exception ex) {
                _logger.LogError(ex, $"❌ Erro inesperado ao mover lead {leadId}");
            }
        }







        public async Task ProcessarAsync(GrupoAtualizadoDTO dto) {
            _logger.LogInformation($"🔔 GrupoAtualizado recebido para GrupoId={dto.GrupoId}");

            var usuario = await _context.Usuario
                .FirstOrDefaultAsync(u => u.GrupoId == dto.GrupoId);

            if (usuario == null) {
                _logger.LogWarning($"⚠️ Nenhum usuário encontrado para GrupoId={dto.GrupoId}");
                return;
            }

            _logger.LogInformation($"👤 Usuário encontrado: {usuario.Email}");

            var lead = await _context.Leads
                .FirstOrDefaultAsync(l =>
                    (!string.IsNullOrEmpty(usuario.Email) && l.Email == usuario.Email) ||
                    (!string.IsNullOrEmpty(usuario.Celular) && l.Telefone == usuario.Celular));

            if (lead == null) {
                _logger.LogInformation($"ℹ️ Nenhum lead encontrado para Email={usuario.Email} / Celular={usuario.Celular}. Nada a mover.");
                return;
            }

            _logger.LogInformation($"📋 Lead encontrado: {lead.LeadId}");

            _logger.LogInformation($"🚀 Movendo lead {lead.LeadId} para statusId={STATUS_CLIENTE} (Cliente)...");
            await MoverLeadAsync(lead.LeadId, STATUS_CLIENTE);
        }






        private string NormalizarTelefone(string telefone) {
            if (string.IsNullOrEmpty(telefone))
                return telefone;

            return telefone
                .Replace("+55", "")
                .Replace(" ", "")
                .Replace("-", "")
                .Replace("(", "")
                .Replace(")", "");
        }







        public async Task ProcessarWebhookAsync(WebhookPayloadDTO payload) {

            var lead = new Leads {
                LeadId = payload.LeadId,
                ContactId = payload.ContactId,
                Nome = payload.Nome,
                Email = payload.Email,
                Telefone = payload.Telefone
            };

            await _leadRepository.SalvarAsync(lead);


            var telefoneNormalizado = NormalizarTelefone(payload.Telefone);

            _logger.LogInformation($"📱 Telefone normalizado: {telefoneNormalizado}");


            var usuario = await _context.Usuario
                .FirstOrDefaultAsync(u =>
                    (!string.IsNullOrEmpty(payload.Email) && u.Email == payload.Email) ||
                    (!string.IsNullOrEmpty(telefoneNormalizado) && u.Celular == telefoneNormalizado));

            if (usuario == null) {
                _logger.LogInformation($"ℹ️ Nenhum usuário encontrado para Email={payload.Email} / Celular={telefoneNormalizado}. Lead aguardando.");
                return;
            }

            _logger.LogInformation($"👤 Usuário encontrado: GrupoId={usuario.GrupoId}");


            var terminal = await _context.Terminal
                .FirstOrDefaultAsync(t => t.GrupoId == usuario.GrupoId);

            if (terminal == null) {
                _logger.LogInformation($"ℹ️ Nenhum terminal encontrado para GrupoId={usuario.GrupoId}.");
                return;
            }

            _logger.LogInformation($"🖥️ Terminal encontrado. GrupoTi={terminal.GrupoTi}");


            long? statusIdDestino = null;

            if (terminal.GrupoTi == 1)
                statusIdDestino = STATUS_CLIENTE;
            else if (terminal.GrupoTi == 3)
                statusIdDestino = STATUS_TESTANDO;

            if (statusIdDestino == null) {
                _logger.LogInformation($"ℹ️ GrupoTi={terminal.GrupoTi} não requer movimentação no Kommo.");
                return;
            }


            _logger.LogInformation($"🚀 Movendo lead {payload.LeadId} para statusId={statusIdDestino.Value}...");
            await MoverLeadAsync(payload.LeadId, statusIdDestino.Value);
        }



    }

}


