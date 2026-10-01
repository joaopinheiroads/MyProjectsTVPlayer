namespace CanvaAPI.Models;

public class ProgramacaoMidiaItem
{
    public int Posicao { get; set; }
    public int ProgramacaoVideoId { get; set; }
    public string? Nome { get; set; }
    public string Tipo { get; set; } = string.Empty;
}
