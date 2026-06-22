using Microsoft.AspNetCore.Mvc;
using EscolhaAI.Services;

namespace EscolhaAI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [IgnoreAntiforgeryToken]
    public class EmailController : ControllerBase
    {
        private readonly EmailService _emailService;

        public EmailController(EmailService emailService)
        {
            _emailService = emailService;
        }


        [HttpGet]
        public IActionResult EmailGet()
        {
            return Ok(new { success = "E-mail enviado com sucesso" });

        }



        [HttpPost("send")]
        public IActionResult Send([FromBody] EmailRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.From) || string.IsNullOrWhiteSpace(request.To))
                return BadRequest("Campos 'From' e 'To' são obrigatórios e não podem ser vazios.");

            try
            {
                bool enviado = _emailService.SendEmail(
                    request.Subject,
                    request.Body,
                    request.From,
                    request.To,
                    request.Bcc
                );
                return Ok(new { sucesso = true });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, error = ex.Message });
            }
        }
    }

    public class EmailRequest
    {
        public string Subject { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public string From { get; set; } = string.Empty;
        public string To { get; set; } = string.Empty;
        public List<string>? Bcc { get; set; }
    }
}
