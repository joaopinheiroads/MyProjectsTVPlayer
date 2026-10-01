namespace CanvaAPI.Models;

public class InserirMidiaResponse
{
    public bool Success { get; set; }
    public int ProgramacaoVideoId { get; set; }
    public int ProgramacaoId { get; set; }
    public int VideoId { get; set; }
    public int? Ordem { get; set; }
    public int? Duracao { get; set; }
    public string? Error { get; set; }
}
