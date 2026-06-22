namespace Cardápio.Infra.Model
{
    public class CodigoVerificacao
    {
        public int ID { get; set; }
        public string Codigo { get; set; }
        public string? Email { get; set; }
        public string? Celular { get; set; }
        public bool Ativo { get; set; }
        public DateTime DataCadastro { get; set; }
        public DateTime DataExpiracao { get; set; }
        public DateTime? DataEdicao { get; set; }
    }
}
