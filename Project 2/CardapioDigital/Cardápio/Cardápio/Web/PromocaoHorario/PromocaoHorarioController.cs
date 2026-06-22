using Cardápio.Domain.PromocaoHorario;
using Cardápio.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Cardápio.Web.PromocaoHorario
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PromocaoHorarioController : ControllerBase
    {
        private readonly PromocaoHorarioService _promocaoHorarioService;

        public PromocaoHorarioController(PromocaoHorarioService promocaoHorarioService)
        {
            _promocaoHorarioService = promocaoHorarioService;
        }

        [HttpGet("promocao/{promocaoId}")]
        public async Task<ActionResult<List<PromocaoHorarioGetDTO>>> GetHorariosByPromocaoId(int promocaoId)
        {
            try
            {
                var horarios = await _promocaoHorarioService.GetHorariosByPromocaoId(promocaoId);
                return Ok(horarios);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("empresa/{empresaId}")]
        public async Task<ActionResult<List<PromocaoHorarioGetDTO>>> GetHorariosByEmpresa(int empresaId)
        {
            try
            {
                var horarios = await _promocaoHorarioService.GetHorariosByEmpresa(empresaId);
                return Ok(horarios);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PromocaoHorarioGetDTO>> GetById(int id)
        {
            try
            {
                var horario = await _promocaoHorarioService.GetById(id);
                if (horario == null)
                    return NotFound();

                return Ok(horario);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<ActionResult<PromocaoHorarioGetDTO>> Add([FromBody] PromocaoHorarioAddDTO dto)
        {
            try
            {
                var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userIdString))
                    return BadRequest("Usuário não autenticado.");

                int userId = int.Parse(userIdString);
                var horario = await _promocaoHorarioService.Add(dto, userId);
                return CreatedAtAction(nameof(GetById), new { id = horario.ID }, horario);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<PromocaoHorarioGetDTO>> Update(int id, [FromBody] PromocaoHorarioUpdateDTO dto)
        {
            try
            {
                if (id != dto.ID)
                    return BadRequest("ID do horário não confere.");

                var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userIdString))
                    return BadRequest("Usuário não autenticado.");

                int userId = int.Parse(userIdString);
                var horario = await _promocaoHorarioService.Update(dto, userId);
                return Ok(horario);
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
                var result = await _promocaoHorarioService.Delete(id);
                if (!result)
                    return NotFound();

                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("ativos")]
        public async Task<ActionResult<List<PromocaoHorarioGetDTO>>> GetHorariosAtivos()
        {
            try
            {
                var dataAtual = DateTime.Now;
                var diaSemana = GetDiaSemanaPortugues(dataAtual.DayOfWeek);
                var horaAtual = dataAtual.ToString("HH:mm");

                var horarios = await _promocaoHorarioService.GetHorariosAtivos(dataAtual, diaSemana, horaAtual);
                return Ok(horarios);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        private static string GetDiaSemanaPortugues(DayOfWeek dayOfWeek)
        {
            return dayOfWeek switch
            {
                DayOfWeek.Monday => "Segunda",
                DayOfWeek.Tuesday => "Terça",
                DayOfWeek.Wednesday => "Quarta",
                DayOfWeek.Thursday => "Quinta",
                DayOfWeek.Friday => "Sexta",
                DayOfWeek.Saturday => "Sábado",
                DayOfWeek.Sunday => "Domingo",
                _ => ""
            };
        }
    }
}
