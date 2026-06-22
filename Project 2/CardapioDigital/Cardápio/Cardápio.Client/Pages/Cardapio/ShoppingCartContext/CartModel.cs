namespace Cardápio.Client.Pages.Cardapio.ShoppingCartContext
{
    public class CartModel
    {
        public string ItemCartId { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public decimal? OriginalPrice { get; set; } // Preço original (sem desconto)
        public bool HasPromotion { get; set; } // Se o item está em promoção
        public string ImageUrl { get; set; }
        public int Quantity { get; set; }
        public List<CartAdditionalGroupModel> AdditionalGroups { get; set; } = new List<CartAdditionalGroupModel>();
        public string Observation { get; set; }
    }

    public class CartAdditionalGroupModel
    {
        public string GroupName { get; set; }
        public List<CartAdditionalItemModel> Items { get; set; }
        public decimal GroupTotalPrice { get; set; }
    }

    public class CartAdditionalItemModel
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal TotalPrice { get; set; }
    }
}
