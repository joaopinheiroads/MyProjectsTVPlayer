using Cardápio.Domain.QrCode;
using Cardápio.Dto;
using Cardápio.Infra.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cardápio.Web.QrCodeLayout
{
    [Route("api/qrcode")]
    [ApiController]
    public class QrCodeLayoutController : ControllerBase
    {
        private readonly QrCodeService _qrCodeService;

        public QrCodeLayoutController(QrCodeService qrCodeService)
        {
            _qrCodeService = qrCodeService;
        }

        [Authorize]
        [HttpGet("data/{empresaID}")]
        public async Task<IActionResult> GetQrCodeLayouts(int empresaID)
        {
            return await HandleServiceResponse(() => _qrCodeService.GetQrCodeLayouts(User, empresaID));
        }

        [Authorize]
        [HttpPost("data/{empresaID}")]
        public async Task<IActionResult> CreateQrCodeLayout(int empresaID, QrCodeLayoutAddDTO qrCodeLayout)
        {
            return await HandleServiceResponse(() => _qrCodeService.CreateQrCodeLayout(User, empresaID, qrCodeLayout));
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
