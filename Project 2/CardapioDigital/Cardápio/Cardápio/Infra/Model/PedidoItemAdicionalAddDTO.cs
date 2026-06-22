namespace Cardápio.Infra.Model
{
    public class PedidoItemAdicionalAddDTO
    {
        public int GrupoAdicionalItemID { get; set; }
        public int Quantidade { get; set; }
        public decimal PrecoUnitario { get; set; }
        public decimal Subtotal { get; set; }
    }
}
