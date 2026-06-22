namespace Cardápio.Dto
{
    public class PromocaoHorarioAddDTO
    {
        public int PromocaoID { get; set; }
        public string HoraInicio { get; set; } = "";
        public string HoraFim { get; set; } = "";
        public string DiaSemana { get; set; } = "";
        public bool Ativo { get; set; } = true;
    }

    public class PromocaoHorarioUpdateDTO
    {
        public int ID { get; set; }
        public int PromocaoID { get; set; }
        public string HoraInicio { get; set; } = "";
        public string HoraFim { get; set; } = "";
        public string DiaSemana { get; set; } = "";
        public bool Ativo { get; set; } = true;
    }

    public class PromocaoHorarioGetDTO
    {
        public int ID { get; set; }
        public int PromocaoID { get; set; }
        public string HoraInicio { get; set; } = "";
        public string HoraFim { get; set; } = "";
        public string DiaSemana { get; set; } = "";
        public bool Ativo { get; set; }
        public DateTime DataCadastro { get; set; }
        public DateTime? DataEdicao { get; set; }
        public int UsuarioIDCadastro { get; set; }
        public int? UsuarioIDEdicao { get; set; }
    }
}
