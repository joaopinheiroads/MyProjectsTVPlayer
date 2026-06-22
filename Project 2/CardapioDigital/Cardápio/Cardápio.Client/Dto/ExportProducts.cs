using System.ComponentModel.DataAnnotations;

namespace Cardápio.Client.Dto
{
    public class ExportarProdutosDTO
    {
        public bool ImportCheck { get; set; }

        [Required]
        public int EmpresaIdTo { get; set; }

        public int EmpresaIdFrom { get; set; } // ID da empresa que está exportando os produtos
        public List<ProdutoExportDTO> Produtos { get; set; }
    }

    public class ProdutoExportDTO
    {
        public int ProdutoId { get; set; }
        public decimal Preco { get; set; }

        public List<GrupoAdicionalGetDTO>? GruposID { get; set; }
    }
}
