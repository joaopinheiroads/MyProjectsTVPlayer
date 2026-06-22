using Cardápio.Dto;
using Cardápio.Domain.ProdutoHorario;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Cardápio.Web.ProdutoHorario
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProdutoHorarioController : ControllerBase
    {
        private readonly ProdutoHorarioService _produtoHorarioService;

        public ProdutoHorarioController(ProdutoHorarioService produtoHorarioService)
        {
            _produtoHorarioService = produtoHorarioService;
        }

        [HttpGet("produto/{produtoId}")]
        public async Task<ActionResult<List<ProdutoHorarioGetDTO>>> GetHorariosByProdutoId(int produtoId)
        {
            try
            {
                var horarios = await _produtoHorarioService.GetHorariosByProdutoId(produtoId);
                return Ok(horarios);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("empresa/{empresaId}")]
        public async Task<ActionResult<List<ProdutoHorarioGetDTO>>> GetHorariosByEmpresa(int empresaId)
        {
            try
            {
                var horarios = await _produtoHorarioService.GetHorariosByEmpresa(empresaId);
                return Ok(horarios);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ProdutoHorarioGetDTO>> GetById(int id)
        {
            try
            {
                var horario = await _produtoHorarioService.GetById(id);
                if (horario == null)
                    return NotFound(new { message = "Horário não encontrado." });

                return Ok(horario);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<ActionResult<ProdutoHorarioGetDTO>> Add([FromBody] ProdutoHorarioAddDTO dto)
        {
            try
            {
                var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var horario = await _produtoHorarioService.Add(dto, usuarioId);
                return CreatedAtAction(nameof(GetById), new { id = horario.ID }, horario);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro interno do servidor: " + ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ProdutoHorarioGetDTO>> Update(int id, [FromBody] ProdutoHorarioUpdateDTO dto)
        {
            try
            {
                if (id != dto.ID)
                    return BadRequest(new { message = "ID do parâmetro não confere com o ID do objeto." });

                var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var horario = await _produtoHorarioService.Update(dto, usuarioId);
                return Ok(horario);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro interno do servidor: " + ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                var success = await _produtoHorarioService.Delete(id);
                if (!success)
                    return NotFound(new { message = "Horário não encontrado." });

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro interno do servidor: " + ex.Message });
            }
        }

        [HttpGet("produto/{produtoId}/disponivel")]
        [AllowAnonymous] // Permite acesso sem autenticação para verificar disponibilidade no cardápio
        public async Task<ActionResult<bool>> IsProdutoDisponivelNoHorario(int produtoId, [FromQuery] DateTime? dataHora = null)
        {
            try
            {
                var disponivel = await _produtoHorarioService.IsProdutoDisponivelNoHorario(produtoId, dataHora);
                return Ok(new { disponivel });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("verificar-disponibilidade")]
        [AllowAnonymous] // Permite acesso sem autenticação para verificar disponibilidade no cardápio
        public async Task<ActionResult<List<int>>> GetProdutosDisponiveisNoHorario([FromBody] List<int> produtoIds, [FromQuery] DateTime? dataHora = null)
        {
            try
            {
                var produtosDisponiveis = await _produtoHorarioService.GetProdutosDisponiveisNoHorario(produtoIds, dataHora);
                return Ok(produtosDisponiveis);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
