namespace Cardápio.Dto
{
    public class PromocaoAddDTO
    {
        public string Nome { get; set; } = string.Empty;
        public string? Descricao { get; set; }
        public DateTime DataInicio { get; set; }
        public DateTime DataFim { get; set; }
        public bool Ativo { get; set; } = true;
        public int EmpresaID { get; set; }
        public List<PromocaoHorarioAddDTO> Horarios { get; set; } = new List<PromocaoHorarioAddDTO>();
        public List<ProdutoPromocaoAddDTO> Produtos { get; set; } = new List<ProdutoPromocaoAddDTO>();
    }

    public class PromocaoUpdateDTO
    {
        public int ID { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string? Descricao { get; set; }
        public DateTime DataInicio { get; set; }
        public DateTime DataFim { get; set; }
        public bool Ativo { get; set; } = true;
        public List<PromocaoHorarioUpdateDTO> Horarios { get; set; } = new List<PromocaoHorarioUpdateDTO>();
        public List<ProdutoPromocaoUpdateDTO> Produtos { get; set; } = new List<ProdutoPromocaoUpdateDTO>();
    }

    public class PromocaoGetDTO
    {
        public int ID { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string? Descricao { get; set; }
        public DateTime DataInicio { get; set; }
        public DateTime DataFim { get; set; }
        public bool Ativo { get; set; }
        public int EmpresaID { get; set; }
        public DateTime DataCadastro { get; set; }
        public DateTime? DataEdicao { get; set; }
        public int UsuarioIDCadastro { get; set; }
        public int? UsuarioIDEdicao { get; set; }
        public List<PromocaoHorarioGetDTO> Horarios { get; set; } = new List<PromocaoHorarioGetDTO>();
        public List<ProdutoPromocaoGetDTO> Produtos { get; set; } = new List<ProdutoPromocaoGetDTO>();
    }
}
