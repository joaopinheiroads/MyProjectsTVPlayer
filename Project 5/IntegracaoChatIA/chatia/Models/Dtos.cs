namespace ChatIA.Models;

public class ChatRequest
{
    public string? Message { get; set; }
    public List<HistoryItem>? History { get; set; }
    public string? SessionId { get; set; }
}

public class HistoryItem
{
    public string? Role { get; set; }
    public string? Content { get; set; }
}

public class FaqRequest
{
    public long Id { get; set; }
    public string? Pergunta { get; set; }
    public string? Resposta { get; set; }
    public bool? Ativo { get; set; }
    public string? VideoUrl { get; set; }
}
