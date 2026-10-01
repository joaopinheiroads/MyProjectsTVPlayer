namespace CanvaAPI.Models;

public class ProgramacaoMidiasResponse
{
    public bool Success { get; set; }
    public List<ProgramacaoMidiaItem> Midias { get; set; } = new();
    public string? Error { get; set; }
}
