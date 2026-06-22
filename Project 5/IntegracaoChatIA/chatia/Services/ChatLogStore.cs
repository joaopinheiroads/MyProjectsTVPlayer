using System.Data;
using Microsoft.Data.SqlClient;

namespace ChatIA.Services;

// Grava a pergunta (mascarada) no log. Fire-and-forget: nunca derruba o chat.
public class ChatLogStore
{
    private readonly Db _db;
    private readonly ILogger<ChatLogStore> _log;

    public ChatLogStore(Db db, ILogger<ChatLogStore> log) { _db = db; _log = log; }

    public async Task LogAsync(string pergunta, string? sessionId, string? modelo)
    {
        if (!_db.Enabled) return;
        try
        {
            var masked = Masker.Mask(pergunta);
            if (string.IsNullOrEmpty(masked)) return;
            if (masked.Length > 4000) masked = masked[..4000];

            await using var c = await _db.OpenAsync();
            await using var cmd = new SqlCommand("dbo.usp_ChatLog_Insert", c) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@Pergunta", masked);
            cmd.Parameters.AddWithValue("@SessionId", (object?)sessionId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Modelo", (object?)modelo ?? DBNull.Value);
            await cmd.ExecuteNonQueryAsync();
        }
        catch (Exception ex) { _log.LogError("log falhou (chat segue): {m}", ex.Message); }
    }
}
