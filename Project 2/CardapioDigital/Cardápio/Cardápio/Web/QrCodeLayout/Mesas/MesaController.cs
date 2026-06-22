using Cardápio.Domain.MesaService;
using Cardápio.Dto;
using Cardápio.Infra.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cardápio.Web.QrCodeLayout.Mesas
{
    [ApiController]
    [Route("api/mesa")]
    public class MesaController : ControllerBase
    {
        private readonly MesaService _mesaService;

        public MesaController(MesaService mesaService)
        {
            _mesaService = mesaService;
        }

        [HttpGet("data/{companyID}")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<MesaGetDTO>>> GetDataMesa(int companyID)
        {
            return await HandleServiceResponse(() => _mesaService.GetMesas(User, companyID));
        }

        [HttpPost("data/{empresaID}")]
        [Authorize]
        public async Task<ActionResult> CreateNewMesa(int empresaID)
        {
            return await HandleServiceResponse(() => _mesaService.CreateNewMesa(User, empresaID));
        }

        [HttpPut("data/{empresaID}")]
        [Authorize]
        public async Task<ActionResult> UpdateMesa(int empresaID, MesaUpdateDTO mesaUpdateDTO)
        {
            return await HandleServiceResponse(() => _mesaService.EditMesa(User, empresaID, mesaUpdateDTO));
        }

        [HttpDelete("data/{mesaID}/{empresaID}")]
        [Authorize]
        public async Task<ActionResult> DeleteMesa(int mesaID, int empresaID)
        {
            return await HandleServiceResponse(() => _mesaService.DeleteMesa(User, mesaID, empresaID));
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
