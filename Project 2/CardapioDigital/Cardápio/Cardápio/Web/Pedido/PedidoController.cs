using Cardápio.Domain.Pedido;
using Cardápio.Dto;
using Cardápio.Infra.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cardápio.Web.Order
{
    [Route("api/pedido")]
    [ApiController]
    public class PedidoController : ControllerBase
    {
        private readonly PedidoService _pedidoService;

        public PedidoController(PedidoService pedidoService)
        {
            _pedidoService = pedidoService;
        }

        [Authorize]
        [HttpGet("data/{empresaID}")]
        public async Task<ActionResult<IEnumerable<TodosPedidoGetDTO>>> GetAllPedidos(int empresaID)
        {
            return await HandleServiceResponse(() => _pedidoService.GetAllPedidos(User, empresaID));
        }

        [Authorize]
        [HttpPost("data/{empresaID}")]
        public async Task<ActionResult<IEnumerable<PedidoAddDTO>>> CreateNewPedido(int empresaID, PedidoAddDTO pedidoAddDTO)
        {
            return await HandleServiceResponse(() => _pedidoService.CreateNewPedido(User, empresaID, pedidoAddDTO));
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
