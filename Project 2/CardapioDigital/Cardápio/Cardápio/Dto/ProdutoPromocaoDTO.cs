namespace Cardápio.Dto
{
    public class ProdutoPromocaoAddDTO
    {
        public int ProdutoID { get; set; }
        public int PromocaoID { get; set; }
        public decimal PrecoPromocional { get; set; }
        public bool Ativo { get; set; } = true;
    }

    public class ProdutoPromocaoUpdateDTO
    {
        public int ID { get; set; }
        public int ProdutoID { get; set; }
        public int PromocaoID { get; set; }
        public decimal PrecoPromocional { get; set; }
        public bool Ativo { get; set; } = true;
    }

    public class ProdutoPromocaoGetDTO
    {
        public int ID { get; set; }
        public int ProdutoID { get; set; }
        public int PromocaoID { get; set; }
        public decimal PrecoPromocional { get; set; }
        public bool Ativo { get; set; }
        public DateTime DataCadastro { get; set; }
        public DateTime? DataEdicao { get; set; }
        public int UsuarioIDCadastro { get; set; }
        public int? UsuarioIDEdicao { get; set; }
        
        // Propriedades navegacionais para facilitar o uso
        public string? ProdutoNome { get; set; }
        public string? PromocaoNome { get; set; }
    }
}
