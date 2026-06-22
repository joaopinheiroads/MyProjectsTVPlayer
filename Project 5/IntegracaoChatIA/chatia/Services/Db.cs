using Microsoft.Data.SqlClient;

namespace ChatIA.Services;

// Monta a connection string a partir da config (mesmas variáveis do .env do Node).
public class Db
{
    private readonly string _cs;
    public bool Enabled { get; }

    public Db(IConfiguration cfg)
    {
        var user = cfg["DB_USER"];
        Enabled = !string.IsNullOrEmpty(user);
        var server = cfg["DB_SERVER"] ?? "127.0.0.1";
        var port = cfg["DB_PORT"] ?? "1433";
        var database = cfg["DB_DATABASE"] ?? "VideoLog";
        var pwd = cfg["DB_PASSWORD"] ?? "";
        _cs = $"Data Source={server},{port};Initial Catalog={database};User ID={user};Password={pwd};" +
              "TrustServerCertificate=True;Encrypt=False;Connect Timeout=15";
    }

    public async Task<SqlConnection> OpenAsync()
    {
        var c = new SqlConnection(_cs);
        await c.OpenAsync();
        return c;
    }
}
