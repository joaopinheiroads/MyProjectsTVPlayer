namespace Cardápio.Client.Dto
{
    public class PedidoItemAdicionalAddDTO
    {
        public int GrupoAdicionalItemID { get; set; }
        public string? Nome { get; set; }
        public int Quantidade { get; set; }
        public decimal PrecoUnitario { get; set; }
        public decimal Subtotal { get; set; }
    }
}
