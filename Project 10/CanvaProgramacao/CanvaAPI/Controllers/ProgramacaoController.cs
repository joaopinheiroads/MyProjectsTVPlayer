using CanvaAPI.Models;
using CanvaAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace CanvaAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProgramacaoController : ControllerBase
{
    private readonly ITVPlayerService _tvPlayerService;
    private readonly ILogger<ProgramacaoController> _logger;

    public ProgramacaoController(ITVPlayerService tvPlayerService, ILogger<ProgramacaoController> logger)
    {
        _tvPlayerService = tvPlayerService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Listar()
    {
        try
        {
            var token = LerToken();
            if (string.IsNullOrEmpty(token))
                return Unauthorized(new { success = false, error = "Autenticação necessária" });

            var result = await _tvPlayerService.GetProgramacoesAsync(token);

            if (!result.Success)
            {
                _logger.LogWarning("TVPlayer recusou a listagem de programações: {Error}", result.Error);
                return StatusCode(502, result);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in Listar endpoint");
            return StatusCode(500, new { success = false, error = "Erro ao listar programações" });
        }
    }

    [HttpGet("{programacaoId:int}/midias")]
    public async Task<IActionResult> ListarMidias(int programacaoId)
    {
        try
        {
            var token = LerToken();
            if (string.IsNullOrEmpty(token))
                return Unauthorized(new { success = false, error = "Autenticação necessária" });

            var result = await _tvPlayerService.GetMidiasDaProgramacaoAsync(programacaoId, token);

            if (!result.Success)
            {
                _logger.LogWarning("TVPlayer recusou a listagem das mídias da programação {ProgramacaoId}: {Error}", programacaoId, result.Error);
                return StatusCode(502, result);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in ListarMidias endpoint");
            return StatusCode(500, new { success = false, error = "Erro ao listar as mídias da programação" });
        }
    }

    [HttpPost("{programacaoId:int}/midia")]
    public async Task<IActionResult> InserirMidia(int programacaoId, [FromBody] InserirMidiaRequest midia)
    {
        try
        {
            var token = LerToken();
            if (string.IsNullOrEmpty(token))
                return Unauthorized(new { success = false, error = "Autenticação necessária" });

            if (midia == null || midia.VideoId <= 0)
                return BadRequest(new { success = false, error = "videoId é obrigatório" });

            var result = await _tvPlayerService.InserirMidiaNaProgramacaoAsync(programacaoId, midia, token);

            if (!result.Success)
            {
                _logger.LogWarning("TVPlayer recusou a inserção na programação: {Error}", result.Error);
                return StatusCode(502, result);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in InserirMidia endpoint");
            return StatusCode(500, new { success = false, error = "Erro ao inserir a mídia na programação" });
        }
    }

    private string LerToken()
    {
        return Request.Headers.Authorization.ToString().Replace("Bearer ", "").Trim();
    }
}
