namespace Cardápio.Client.Pages.Cardapio.ShoppingCartContext
{
    public class ShoppingCartContextService
    {
        private List<CartModel> _cartItems = new List<CartModel>();

        public event Action OnCartUpdated;

        public int TotalItems => _cartItems.Sum(item => item.Quantity);
        public int TotalUniqueItems => _cartItems.Count;

        public Task AddToCartAsync(CartModel produto)
        {
            
            if (!string.IsNullOrEmpty(produto.ItemCartId))
            {
                var itemToEdit = _cartItems.FirstOrDefault(i => i.ItemCartId == produto.ItemCartId);
                if (itemToEdit != null)
                {
                    return EditCartItemAsync(produto);
                }
            }

            var existingItem = _cartItems.FirstOrDefault(i =>
                i.Id == produto.Id &&
                i.Observation == produto.Observation &&
                AreAdditionalGroupsEqual(i.AdditionalGroups, produto.AdditionalGroups));

            if (existingItem != null)
            {
                existingItem.Quantity += produto.Quantity;
                OnCartUpdated?.Invoke();
                return Task.CompletedTask;
            }

            _cartItems.Add(new CartModel
            {
                ItemCartId = Guid.NewGuid().ToString(),
                Id = produto.Id,
                Name = produto.Name,
                Price = produto.Price,
                OriginalPrice = produto.OriginalPrice,
                HasPromotion = produto.HasPromotion,
                ImageUrl = produto.ImageUrl,
                Quantity = produto.Quantity,
                Observation = produto.Observation,
                AdditionalGroups = produto.AdditionalGroups
            });

            OnCartUpdated?.Invoke();
            return Task.CompletedTask;
        }

        private bool AreAdditionalGroupsEqual(List<CartAdditionalGroupModel> group1, List<CartAdditionalGroupModel> group2)
        {
            if (group1 == null && group2 == null) return true;
            if (group1 == null || group2 == null) return false;
            if (group1.Count != group2.Count) return false;

            foreach (var g1 in group1)
            {
                var matchingGroup = group2.FirstOrDefault(g2 => g2.GroupName == g1.GroupName && g2.GroupTotalPrice == g1.GroupTotalPrice);
                if (matchingGroup == null || !AreAdditionalItemsEqual(g1.Items, matchingGroup.Items))
                {
                    return false;
                }
            }

            return true;
        }

        private bool AreAdditionalItemsEqual(List<CartAdditionalItemModel> items1, List<CartAdditionalItemModel> items2)
        {
            if (items1 == null && items2 == null) return true;
            if (items1 == null || items2 == null) return false;
            if (items1.Count != items2.Count) return false;

            foreach (var i1 in items1)
            {
                var matchingItem = items2.FirstOrDefault(i2 =>
                    i2.Name == i1.Name &&
                    i2.UnitPrice == i1.UnitPrice &&
                    i2.Quantity == i1.Quantity &&
                    i2.TotalPrice == i1.TotalPrice);

                if (matchingItem == null)
                {
                    return false;
                }
            }

            return true;
        }

        public Task EditCartItemAsync(CartModel produto)
        {
            var itemToEdit = _cartItems.FirstOrDefault(i => i.ItemCartId == produto.ItemCartId);
            if (itemToEdit != null)
            {
                itemToEdit.Quantity = produto.Quantity;
                itemToEdit.Observation = produto.Observation;
                itemToEdit.AdditionalGroups = produto.AdditionalGroups;
                OnCartUpdated?.Invoke();
            }

            return Task.CompletedTask;
        }

        public Task RemoveFromCartAsync(string itemCartId)
        {
            var itemToRemove = _cartItems.FirstOrDefault(i => i.ItemCartId == itemCartId);
            if (itemToRemove != null)
            {
                _cartItems.Remove(itemToRemove);
                OnCartUpdated?.Invoke();
            }

            return Task.CompletedTask;
        }

        public Task IncreaseQuantityAsync(string itemCartId)
        {
            var item = _cartItems.FirstOrDefault(i => i.ItemCartId == itemCartId);
            if (item != null)
            {
                item.Quantity++;
                OnCartUpdated?.Invoke();
            }
            return Task.CompletedTask;
        }

        public Task DecreaseQuantityAsync(string itemCartId)
        {
            var item = _cartItems.FirstOrDefault(i => i.ItemCartId == itemCartId);
            if (item != null)
            {
                if (item.Quantity > 1)
                {
                    item.Quantity--;
                }
                else
                {
                    _cartItems.Remove(item);
                }

                OnCartUpdated?.Invoke();
            }
            return Task.CompletedTask;
        }

        public Task<List<CartModel>> GetCartItemsAsync()
        {
            return Task.FromResult(_cartItems.ToList());
        }

        public Task<decimal> GetTotalValueAsync()
        {
            var total = _cartItems.Sum(item =>
                (item.Price * item.Quantity) + (item.AdditionalGroups?.Sum(group => group.Items?.Sum(add => add.TotalPrice) ?? 0) ?? 0));
            return Task.FromResult(total);
        }

        public Task ClearCartAsync()
        {
            _cartItems.Clear();
            OnCartUpdated?.Invoke();
            return Task.CompletedTask;
        }
    }
}