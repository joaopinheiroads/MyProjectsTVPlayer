namespace Cardápio.Client.Dto
{
    public class ProdutoUpdateDTO
    {
        public string? Nome { get; set; }
        public decimal Preco { get; set; }
        public string? Descricao { get; set; }
        public int CategoriaID { get; set; }
        public bool Promocao { get; set; }
        public decimal? PrecoPromocional { get; set; }
        public DateTime? FimPromocao { get; set; }
        public bool Destaque { get; set; }
        public bool Ativo { get; set; }
        public List<ProdutoHorarioAddDTO> Horarios { get; set; } = new List<ProdutoHorarioAddDTO>();
        // Nova propriedade para promoções por horário
        public List<ProdutoPromocaoHorarioAddDTO> PromocoesPorHorario { get; set; } = new List<ProdutoPromocaoHorarioAddDTO>();
        // Adicione outros campos necessários conforme o backend
    }
}
