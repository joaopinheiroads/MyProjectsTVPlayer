using System.ComponentModel.DataAnnotations;

namespace Cardápio.Client.Dto
{
    public class ProdutoHorarioAddDTO
    {
        public int ProdutoID { get; set; }

        [Required(ErrorMessage = "Hora de início é obrigatória")]
        [MaxLength(8)]
        public string HoraInicio { get; set; } = "";

        [Required(ErrorMessage = "Hora de fim é obrigatória")]
        [MaxLength(8)]
        public string HoraFim { get; set; } = "";

        [Required(ErrorMessage = "Dia da semana é obrigatório")]
        public string DiaSemana { get; set; } = "";

        public bool Ativo { get; set; } = true;
    }

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

    public class ProdutoHorarioUpdateDTO
    {
        public int ID { get; set; }
        public string HoraInicio { get; set; } = "";
        public string HoraFim { get; set; } = "";
        public string DiaSemana { get; set; } = "";
        public bool Ativo { get; set; }
    }
}
