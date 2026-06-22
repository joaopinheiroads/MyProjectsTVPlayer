using Cardápio.Infra.Model;

namespace Cardápio.Dto
{
    public class PedidoAddDTO
    {
        public string UsuarioClienteNome { get; set; }
        public string? NomeMesa { get; set; }
        public int EmpresaID { get; set; }
        public string EmpresaNome { get; set; }
        public int StatusPedidoID { get; set; }
        public string EmpresaImageUrl { get; set; }
        public decimal Total { get; set; }
        public List<PedidoItemAddDTO> ColPedidoItem { get; set; }

        // Propriedade para armazenar a URL de origem do cardápio
        public string? UrlOrigemCardapio { get; set; }
    }
}
