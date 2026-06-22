using Cardápio.Domain.Promocao;
using Cardápio.Dto;
using Cardápio.Infra.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Cardápio.Web.Promocao
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PromocaoController : ControllerBase
    {
        private readonly PromocaoService _promocaoService;

        public PromocaoController(PromocaoService promocaoService)
        {
            _promocaoService = promocaoService;
        }

        [HttpGet("empresa/{empresaId}")]
        public async Task<ActionResult<List<PromocaoGetDTO>>> GetPromocoesByEmpresa(int empresaId)
        {
            try
            {
                var promocoes = await _promocaoService.GetPromocoesByEmpresa(empresaId);
                return Ok(promocoes);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PromocaoGetDTO>> GetById(int id)
        {
            try
            {
                var promocao = await _promocaoService.GetById(id);
                if (promocao == null)
                    return NotFound();

                return Ok(promocao);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<ActionResult<PromocaoGetDTO>> Add([FromBody] PromocaoAddDTO dto)
        {
            try
            {
                var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userIdString))
                    return BadRequest("Usuário não autenticado.");

                int userId = int.Parse(userIdString);
                var promocao = await _promocaoService.Add(dto, userId);
                return CreatedAtAction(nameof(GetById), new { id = promocao.ID }, promocao);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<PromocaoGetDTO>> Update(int id, [FromBody] PromocaoUpdateDTO dto)
        {
            try
            {
                if (id != dto.ID)
                    return BadRequest("ID da promoção não confere.");

                var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userIdString))
                    return BadRequest("Usuário não autenticado.");

                int userId = int.Parse(userIdString);
                var promocao = await _promocaoService.Update(dto, userId);
                return Ok(promocao);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                var result = await _promocaoService.Delete(id);
                if (!result)
                    return NotFound();

                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("ativas/empresa/{empresaId}")]
        public async Task<ActionResult<List<PromocaoGetDTO>>> GetPromocoesAtivas(int empresaId)
        {
            try
            {
                var promocoes = await _promocaoService.GetPromocoesAtivas(empresaId);
                return Ok(promocoes);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("produto/{produtoId}/preco-promocional")]
        public async Task<ActionResult<decimal?>> GetPrecoPromocionalProduto(int produtoId)
        {
            try
            {
                var precoPromocional = await _promocaoService.GetPrecoPromocionalProduto(produtoId);
                return Ok(new { precoPromocional });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
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
