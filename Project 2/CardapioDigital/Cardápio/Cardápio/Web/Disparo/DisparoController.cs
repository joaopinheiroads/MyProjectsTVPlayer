
using Cardápio.Domain;
using Microsoft.AspNetCore.Mvc;
using Cardápio.Dto;

namespace Cardápio.Web.Disparo
{

    [ApiController]
    [Route("api/disparos")]
    public class DisparoController : ControllerBase
    {
        private readonly DisparosChatService _disparoService;

        public DisparoController(DisparosChatService disparosService)
        {
            _disparoService = disparosService;
        }

        [HttpPost("enviar")]
        public async Task<IActionResult> EnviarMensagem([FromBody] DisparoMensagemDTO request)
        {
            var resultado = await _disparoService.EnviarMensagemAsync(request.Number, request.Message);

            if (resultado.Sucesso)
                return Ok(new { message = "Mensagem enviada com sucesso!" });

            return BadRequest(new { error = resultado.Erro });
        }
    }
}
