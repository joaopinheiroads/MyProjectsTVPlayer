namespace CanvaAPI.Models;

public class ProgramacoesResponse
{
    public bool Success { get; set; }
    public List<ProgramacaoItem> Programacoes { get; set; } = new();
    public string? Error { get; set; }
}
