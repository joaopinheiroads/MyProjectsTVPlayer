namespace Cardápio.Dto
{
    public class ExportarProdutosDTO
    {
        public bool ImportCheck { get; set; }
        public int EmpresaIdTo { get; set; }  // ID da empresa que irá receber os produtos
        public int EmpresaIdFrom { get; set; } // ID da empresa que está exportando os produtos
        public List<ProdutoExportDTO> Produtos { get; set; }  // Lista dos produtos a serem exportados
    }

    public class ProdutoExportDTO
    {
        public int ProdutoId { get; set; }  // ID do produto
        public decimal Preco { get; set; }  // Preço do produto

        public List<GrupoAdicionalGetDTO>? GruposID { get; set; }
    }
}
