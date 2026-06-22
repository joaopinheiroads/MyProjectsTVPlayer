using System.Data;
using System.Text;
using Microsoft.Data.SqlClient;

namespace ChatIA.Services;

public record FaqItem(long Id, string Pergunta, string Resposta, string? VideoUrl, bool Ativo, DateTime DataCadastro, DateTime? DataAlteracao);

public class FaqStore
{
    private readonly Db _db;
    private readonly ILogger<FaqStore> _log;
    private string _cacheText = "";
    private DateTime _cacheAt = DateTime.MinValue;
    private static readonly TimeSpan Ttl = TimeSpan.FromSeconds(20);
    private readonly SemaphoreSlim _gate = new(1, 1);

    public FaqStore(Db db, ILogger<FaqStore> log) { _db = db; _log = log; }

    // Texto dos FAQs ativos para o prompt (cacheado por ~20s). Falha silenciosa.
    public async Task<string> GetActiveFaqTextAsync()
    {
        if (!_db.Enabled) return "";
        if (DateTime.UtcNow - _cacheAt < Ttl) return _cacheText;
        await _gate.WaitAsync();
        try
        {
            if (DateTime.UtcNow - _cacheAt < Ttl) return _cacheText;
            var rows = await ListActiveAsync();
            _cacheText = BuildText(rows);
            _cacheAt = DateTime.UtcNow;
        }
        catch (Exception ex) { _log.LogError("FAQ read falhou (chat segue): {m}", ex.Message); }
        finally { _gate.Release(); }
        return _cacheText;
    }

    private void Invalidate() => _cacheAt = DateTime.MinValue;

    private static string BuildText(List<(string Pergunta, string Resposta, string? VideoUrl)> rows)
    {
        if (rows.Count == 0) return "";
        var sb = new StringBuilder();
        for (int i = 0; i < rows.Count; i++)
        {
            if (i > 0) sb.Append("\n\n");
            sb.Append($"{i + 1}. P: {rows[i].Pergunta}\n   R: {rows[i].Resposta}");
            if (!string.IsNullOrEmpty(rows[i].VideoUrl)) sb.Append($"\n   [VIDEO_TUTORIAL: {rows[i].VideoUrl}]");
        }
        return sb.ToString();
    }

    private async Task<List<(string, string, string?)>> ListActiveAsync()
    {
        var list = new List<(string, string, string?)>();
        await using var c = await _db.OpenAsync();
        await using var cmd = new SqlCommand("dbo.usp_ChatFaq_ListActive", c) { CommandType = CommandType.StoredProcedure };
        await using var r = await cmd.ExecuteReaderAsync();
        while (await r.ReadAsync())
            list.Add((r["Pergunta"] as string ?? "", r["Resposta"] as string ?? "", r["VideoUrl"] as string));
        return list;
    }

    public async Task<List<FaqItem>> ListAllAsync()
    {
        var list = new List<FaqItem>();
        await using var c = await _db.OpenAsync();
        await using var cmd = new SqlCommand("dbo.usp_ChatFaq_ListAll", c) { CommandType = CommandType.StoredProcedure };
        await using var r = await cmd.ExecuteReaderAsync();
        while (await r.ReadAsync())
            list.Add(new FaqItem(
                Convert.ToInt64(r["Id"]),
                r["Pergunta"] as string ?? "",
                r["Resposta"] as string ?? "",
                r["VideoUrl"] as string,
                Convert.ToBoolean(r["Ativo"]),
                Convert.ToDateTime(r["DataCadastro"]),
                r["DataAlteracao"] == DBNull.Value ? null : Convert.ToDateTime(r["DataAlteracao"])));
        return list;
    }

    public async Task<long?> InsertAsync(string pergunta, string resposta, string? videoUrl)
    {
        await using var c = await _db.OpenAsync();
        await using var cmd = new SqlCommand("dbo.usp_ChatFaq_Insert", c) { CommandType = CommandType.StoredProcedure };
        cmd.Parameters.AddWithValue("@Pergunta", pergunta);
        cmd.Parameters.AddWithValue("@Resposta", resposta);
        cmd.Parameters.AddWithValue("@VideoUrl", (object?)videoUrl ?? DBNull.Value);
        var idObj = await cmd.ExecuteScalarAsync();
        Invalidate();
        return idObj is null || idObj == DBNull.Value ? null : Convert.ToInt64(idObj);
    }

    public async Task UpdateAsync(long id, string pergunta, string resposta, bool ativo, string? videoUrl)
    {
        await using var c = await _db.OpenAsync();
        await using var cmd = new SqlCommand("dbo.usp_ChatFaq_Update", c) { CommandType = CommandType.StoredProcedure };
        cmd.Parameters.AddWithValue("@Id", id);
        cmd.Parameters.AddWithValue("@Pergunta", pergunta);
        cmd.Parameters.AddWithValue("@Resposta", resposta);
        cmd.Parameters.AddWithValue("@Ativo", ativo);
        cmd.Parameters.AddWithValue("@VideoUrl", (object?)videoUrl ?? DBNull.Value);
        await cmd.ExecuteNonQueryAsync();
        Invalidate();
    }

    public async Task DeleteAsync(long id)
    {
        await using var c = await _db.OpenAsync();
        await using var cmd = new SqlCommand("dbo.usp_ChatFaq_Delete", c) { CommandType = CommandType.StoredProcedure };
        cmd.Parameters.AddWithValue("@Id", id);
        await cmd.ExecuteNonQueryAsync();
        Invalidate();
    }
}
