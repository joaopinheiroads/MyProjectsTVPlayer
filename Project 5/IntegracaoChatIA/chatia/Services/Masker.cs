using System.Text.RegularExpressions;

namespace ChatIA.Services;

// Mascara dados sensíveis ANTES de gravar no log (porta de mask.js).
public static class Masker
{
    public static string Mask(string? input)
    {
        if (string.IsNullOrEmpty(input)) return input ?? "";
        var t = input;
        // 1) senha/token/chave = VALOR -> oculta o valor
        t = Regex.Replace(t, @"\b(senhas?|password|pwd|token|secret|api[\s_-]?key|chave|bearer)\b\s*[:=]?\s*\S+", "$1: [oculto]", RegexOptions.IgnoreCase);
        // 2) e-mail
        t = Regex.Replace(t, @"\b[\w.+-]+@[\w-]+\.[\w.-]+\b", "[email]", RegexOptions.IgnoreCase);
        // 3) CNPJ
        t = Regex.Replace(t, @"\b\d{2}\.?\d{3}\.?\d{3}\/?\d{4}-?\d{2}\b", "[cnpj]");
        // 4) cartão
        t = Regex.Replace(t, @"\b\d{4}[ -]?\d{4}[ -]?\d{4}[ -]?\d{1,4}\b", "[cartao]");
        // 5) CPF
        t = Regex.Replace(t, @"\b\d{3}\.?\d{3}\.?\d{3}-?\d{2}\b", "[cpf]");
        // 6) telefone BR
        t = Regex.Replace(t, @"\b(?:\+?55\s?)?(?:\(?\d{2}\)?[\s.-]?)?9?\d{4}[\s.-]?\d{4}\b", "[telefone]");
        return t;
    }
}
