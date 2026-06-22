using System.Security.Cryptography;
using System.Text;

namespace ChatIA.Services;

// Valida o token de admin (HMAC) gerado pelo painel só para o admin master.
// Token: "<uid>.<exp>.<hmacsha256(SECRET, uid.exp)>"
public class AdminAuth
{
    private readonly string _secret;
    private readonly HashSet<string> _allow;

    public AdminAuth(IConfiguration cfg)
    {
        _secret = cfg["ADMIN_TOKEN_SECRET"] ?? "";
        _allow = (cfg["ADMIN_USER_IDS"] ?? "")
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToHashSet();
    }

    public bool Verify(string? token)
    {
        if (string.IsNullOrEmpty(_secret) || string.IsNullOrEmpty(token)) return false;
        var parts = token.Split('.');
        if (parts.Length != 3) return false;
        string uid = parts[0], exp = parts[1], sig = parts[2];
        if (!long.TryParse(uid, out _) || !long.TryParse(exp, out var expSec)) return false;

        using var h = new HMACSHA256(Encoding.UTF8.GetBytes(_secret));
        var expected = Convert.ToHexString(h.ComputeHash(Encoding.UTF8.GetBytes(uid + "." + exp))).ToLowerInvariant();
        if (!CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(sig), Encoding.UTF8.GetBytes(expected))) return false;

        if (expSec * 1000 < DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()) return false; // expirado
        if (_allow.Count > 0 && !_allow.Contains(uid)) return false;
        return true;
    }
}
