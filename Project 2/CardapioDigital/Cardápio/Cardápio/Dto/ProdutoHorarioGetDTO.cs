namespace Cardápio.Dto
{
    public class ProdutoHorarioGetDTO
    {
        public int ID { get; set; }
        public int ProdutoID { get; set; }
        public string HoraInicio { get; set; } = "";
        public string HoraFim { get; set; } = "";
        public string DiaSemana { get; set; } = "";
        public bool Ativo { get; set; }
        public DateTime DataCadastro { get; set; }
    }
}
