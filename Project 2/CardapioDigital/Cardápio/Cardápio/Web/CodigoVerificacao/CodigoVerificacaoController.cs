using Cardápio.Domain.Auth;
using Cardápio.Domain.Codigo;
using Cardápio.Dto;
using Cardápio.Infra.Data;
using Cardápio.Infra.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace Cardápio.Web.Code
{
    [ApiController]
    [Route("api/code")]
    public class CodigoVerificacaoController : ControllerBase
    {
        private readonly CodigoVerificacaoService _codeService;
        private readonly AppDbContext _appDbContext;
        private readonly CardapioUnitOfWork _cardapioUnitOfWork;

        public CodigoVerificacaoController(CodigoVerificacaoService codeService, AppDbContext appDbContext, CardapioUnitOfWork cardapioUnitOfWork)
        {
            _codeService = codeService;
            _appDbContext = appDbContext;
            _cardapioUnitOfWork = cardapioUnitOfWork;
        }

        [HttpPost("verify-code")]
        public async Task<IActionResult> VerifyCode([FromBody] CodigoVerificacaoValueAddDTO code)
        {
            return await HandleServiceResponse(() => _codeService.VerifyCode(code));
        }


        [HttpPost("enviar")]
        public async Task<IActionResult> EnviarCodigo([FromBody] CodigoVerificacaoToAddDTO request)
        {
            return (IActionResult)await _codeService.SendCode(request);
        }

        private async Task<ActionResult> HandleServiceResponse(Func<Task<ActionResult>> serviceMethod)
        {
            try
            {
                ActionResult result = await serviceMethod();

                if (result is OkObjectResult okResult)
                {
                    return Ok(okResult.Value);
                }

                return Ok(result);
            }
            catch (ErrorResponse ex)
            {
                ErrorResponseDTO errorResponseDto = new ErrorResponseDTO(ex);

                return new ObjectResult(errorResponseDto)
                {
                    StatusCode = ex.CodeError
                };
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ErrorResponse($"Erro interno: {ex.Message}", 500));
            }
        }
    }
}
