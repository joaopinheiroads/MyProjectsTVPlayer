namespace Cardápio.Client.Dto
{
    public class MesaGetDTO
    {
        public int ID { get; set; }
        public string NomeMesa { get; set; }
        public string QrCode { get; set; }
        public string MesaStatus { get; set; }
        public DateTime DataCadastro { get; set; }
    }
}
