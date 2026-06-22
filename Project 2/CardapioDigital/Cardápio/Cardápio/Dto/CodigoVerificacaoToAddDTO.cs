namespace Cardápio.Dto
{
    public class CodigoVerificacaoToAddDTO
    {
        public string? Email { get; set; }
        public string? Phone { get; set; }
    }

    public class CodigoVerificacaoValueAddDTO
    {
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string Code { get; set; }
    }
}
