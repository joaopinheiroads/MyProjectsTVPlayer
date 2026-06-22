using System.Text.Json;
using ChatIA.Models;
using ChatIA.Services;
using Microsoft.AspNetCore.Mvc;

namespace ChatIA.Controllers;

[ApiController]
public class ChatController : ControllerBase
{
    private const int MaxMsgLen = 4000;
    private const int MaxHistory = 12;

    private readonly ClaudeService _claude;
    private readonly ChatLogStore _logStore;

    public ChatController(ClaudeService claude, ChatLogStore logStore)
    {
        _claude = claude;
        _logStore = logStore;
    }

    [HttpPost("api/chat")]
    public async Task Chat([FromBody] ChatRequest? req, CancellationToken ct)
    {
        var message = (req?.Message ?? "").Trim();
        if (string.IsNullOrEmpty(message) || message.Length > MaxMsgLen)
        {
            Response.StatusCode = 400;
            await Response.WriteAsJsonAsync(new { error = "Mensagem inválida." }, ct);
            return;
        }

        var history = (req!.History ?? new())
            .Where(m => m != null
                        && (m.Role == "user" || m.Role == "assistant")
                        && !string.IsNullOrEmpty(m.Content)
                        && m.Content!.Length <= MaxMsgLen)
            .TakeLast(MaxHistory)
            .Select(m => (m.Role!, m.Content!))
            .ToList();
        history.Add(("user", message));

        // log da pergunta (mascarado, fire-and-forget — não bloqueia a resposta)
        _ = _logStore.LogAsync(message, req.SessionId, _claude.ModelId);

        Response.ContentType = "text/event-stream; charset=utf-8";
        Response.Headers.CacheControl = "no-cache, no-transform";
        Response.Headers["X-Accel-Buffering"] = "no";

        try
        {
            await _claude.StreamAsync(history, async text =>
            {
                await Response.WriteAsync($"event: delta\ndata: {JsonSerializer.Serialize(new { text })}\n\n", ct);
                await Response.Body.FlushAsync(ct);
            }, ct);

            await Response.WriteAsync("event: done\ndata: {\"ok\":true}\n\n", ct);
            await Response.Body.FlushAsync(ct);
        }
        catch (OperationCanceledException) { /* cliente desconectou */ }
        catch (Exception)
        {
            var payload = JsonSerializer.Serialize(new { message = "Desculpe, ocorreu um erro. Tente novamente em instantes." });
            await Response.WriteAsync($"event: error\ndata: {payload}\n\n", ct);
            await Response.Body.FlushAsync(ct);
        }
    }
}
