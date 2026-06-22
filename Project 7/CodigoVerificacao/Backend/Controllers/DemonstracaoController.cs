using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using TVPlayerSite.API.Interfaces.CRM;
using TVPlayerSite.Models.CRM;

namespace TVPlayerSite.API.CRMControllers
{
   
        [ApiController]
        [Route("api/[controller]")]
        public class DemonstracaoController : ControllerBase
        {
            private readonly IDemonstracaoService _service;

            public DemonstracaoController(IDemonstracaoService service)
            {
                _service = service;
            }

            [HttpPost("enviar-codigo")]
            public async Task<IActionResult> EnviarCodigo([FromBody] EnviarCodigoRequest request)
            {
                var sucesso = await _service.EnviarCodigoPorEmail(
                    request.Email,
                    request.Nome,
                    request.Codigo
                );

                if (sucesso)
                    return Ok(new { mensagem = "Código enviado com sucesso" });

                return BadRequest(new { mensagem = "Erro ao enviar código" });
            }
        }
    }



