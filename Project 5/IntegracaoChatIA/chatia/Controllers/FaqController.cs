using ChatIA.Models;
using ChatIA.Services;
using Microsoft.AspNetCore.Mvc;

namespace ChatIA.Controllers;

[ApiController]
public class FaqController : ControllerBase
{
    private readonly FaqStore _faq;
    private readonly AdminAuth _auth;

    public FaqController(FaqStore faq, AdminAuth auth)
    {
        _faq = faq;
        _auth = auth;
    }

    private bool IsAdmin() => _auth.Verify(Request.Headers["X-Admin-Token"].FirstOrDefault());

    private static string? Clean(string? s, int max)
    {
        var v = (s ?? "").Trim();
        if (v.Length == 0) return null;
        return v.Length > max ? v[..max] : v;
    }

    [HttpGet("api/faq")]
    public async Task<IActionResult> List()
    {
        if (!IsAdmin()) return Unauthorized(new { error = "Não autorizado." });
        return Ok(new { faqs = await _faq.ListAllAsync() });
    }

    [HttpPost("api/faq")]
    public async Task<IActionResult> Add([FromBody] FaqRequest? b)
    {
        if (!IsAdmin()) return Unauthorized(new { error = "Não autorizado." });
        var pergunta = Clean(b?.Pergunta, 1000);
        var resposta = Clean(b?.Resposta, 8000);
        if (pergunta is null || resposta is null)
            return BadRequest(new { error = "Pergunta e resposta são obrigatórias." });

        var id = await _faq.InsertAsync(pergunta, resposta, Clean(b!.VideoUrl, 500));
        return Ok(new { ok = true, id });
    }

    [HttpPost("api/faq/update")]
    public async Task<IActionResult> Update([FromBody] FaqRequest? b)
    {
        if (!IsAdmin()) return Unauthorized(new { error = "Não autorizado." });
        if (b is null || b.Id <= 0) return BadRequest(new { error = "ID inválido." });
        var pergunta = Clean(b.Pergunta, 1000);
        var resposta = Clean(b.Resposta, 8000);
        if (pergunta is null || resposta is null)
            return BadRequest(new { error = "Pergunta e resposta são obrigatórias." });

        await _faq.UpdateAsync(b.Id, pergunta, resposta, b.Ativo != false, Clean(b.VideoUrl, 500));
        return Ok(new { ok = true });
    }

    [HttpPost("api/faq/delete")]
    public async Task<IActionResult> Delete([FromBody] FaqRequest? b)
    {
        if (!IsAdmin()) return Unauthorized(new { error = "Não autorizado." });
        if (b is null || b.Id <= 0) return BadRequest(new { error = "ID inválido." });
        await _faq.DeleteAsync(b.Id);
        return Ok(new { ok = true });
    }
}
