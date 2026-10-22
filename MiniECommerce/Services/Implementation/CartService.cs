using MiniECommerce.Extensions;
using MiniECommerce.Repository.Interfaces;
using MiniECommerce.Services.Interfaces;
using MiniECommerce.ViewModels;

namespace MiniECommerce.Services.Implementation
{
    public class CartService : ICartService
    {
        private const string CartSessionKey = "Shopping_Cart_Session";
        private const int MaxQty = CartViewModel.MaxQuantityPerItem;

        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUnitOfWork _unitOfWork;

        public CartService(IHttpContextAccessor httpContextAccessor, IUnitOfWork unitOfWork)
        {
            _httpContextAccessor = httpContextAccessor;
            _unitOfWork = unitOfWork;
        }

        private ISession Session => _httpContextAccessor.HttpContext!.Session;

        public CartViewModel GetCart()
        {
            return Session.GetObjectFromJson<CartViewModel>(CartSessionKey) ?? new CartViewModel();
        }

        private void SaveCart(CartViewModel cart) => Session.SetObjectAsJson(CartSessionKey, cart);

        public async Task<CartSyncResult> SyncCartAsync()
        {
            var cart = GetCart();
            var notices = new List<string>();

            if (cart.Items.Count == 0)
                return new CartSyncResult { Cart = cart, Notices = notices };

            var ids = cart.Items.Select(i => i.ProductId).Distinct().ToList();
            var products = (await _unitOfWork.Products.FindAsync(p => ids.Contains(p.Id)))
                .ToDictionary(p => p.Id);

            var changed = false;

            foreach (var item in cart.Items.ToList())
            {
                if (!products.TryGetValue(item.ProductId, out var product) || !product.IsActive || product.Stock <= 0)
                {
                    cart.Items.Remove(item);
                    notices.Add($"\"{item.ProductName}\" is no longer available and was removed from your cart.");
                    changed = true;
                    continue;
                }

                var allowed = Math.Min(product.Stock, MaxQty);
                if (item.Quantity > allowed)
                {
                    notices.Add($"Only {allowed} unit(s) of \"{product.Name}\" are available, so the quantity was reduced.");
                    item.Quantity = allowed;
                    changed = true;
                }

                if (item.UnitPrice != product.Price)
                {
                    notices.Add($"The price of \"{product.Name}\" changed from {item.UnitPrice.ToEgp()} to {product.Price.ToEgp()}.");
                    item.UnitPrice = product.Price;
                    changed = true;
                }

                if (item.ProductName != product.Name || item.ImageUrl != product.ImageUrl)
                {
                    item.ProductName = product.Name;
                    item.ImageUrl = product.ImageUrl;
                    changed = true;
                }
            }

            if (changed)
                SaveCart(cart);

            return new CartSyncResult { Cart = cart, Notices = notices };
        }

        public async Task<(bool Success, string Message)> AddToCartAsync(int productId, int quantity)
        {
            // Bounded quantities also remove the int-overflow trick (e.g. quantity = int.MaxValue).
            if (quantity < 1 || quantity > MaxQty)
                return (false, $"Quantity must be between 1 and {MaxQty}.");

            var product = await _unitOfWork.Products.GetByIdAsync(productId);
            if (product == null || !product.IsActive)
                return (false, "Product not available.");

            if (product.Stock <= 0)
                return (false, "Sorry, this product is out of stock.");

            var cart = GetCart();
            var existingItem = cart.Items.FirstOrDefault(i => i.ProductId == productId);
            long newQuantity = (long)(existingItem?.Quantity ?? 0) + quantity;

            if (newQuantity > MaxQty)
                return (false, $"You can order up to {MaxQty} units of the same product.");

            if (newQuantity > product.Stock)
                return (false, $"Requested quantity is not available in stock. Available now: {product.Stock}");

            if (existingItem != null)
            {
                existingItem.Quantity = (int)newQuantity;
                existingItem.UnitPrice = product.Price;
            }
            else
            {
                cart.Items.Add(new CartItemViewModel
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    UnitPrice = product.Price,
                    Quantity = quantity,
                    ImageUrl = product.ImageUrl
                });
            }

            SaveCart(cart);
            return (true, $"\"{product.Name}\" was added to your cart.");
        }

        public async Task<(bool Success, string Message)> UpdateQuantityAsync(int productId, int quantity)
        {
            var cart = GetCart();
            var item = cart.Items.FirstOrDefault(i => i.ProductId == productId);

            if (item == null)
                return (false, "Product not found in cart.");

            // Remove item if quantity is zero or negative
            if (quantity <= 0)
            {
                cart.Items.Remove(item);
                SaveCart(cart);
                return (true, "Product removed from cart.");
            }

            if (quantity > MaxQty)
                return (false, $"You can order up to {MaxQty} units of the same product.");

            // Always check against the latest data in the database
            var product = await _unitOfWork.Products.GetByIdAsync(productId);

            if (product == null || !product.IsActive)
                return (false, "Product is no longer available.");

            if (quantity > product.Stock)
                return (false, $"Only {product.Stock} units are available in stock.");

            item.Quantity = quantity;
            item.UnitPrice = product.Price;
            item.ProductName = product.Name;
            item.ImageUrl = product.ImageUrl;

            SaveCart(cart);
            return (true, "Cart updated.");
        }

        public void RemoveFromCart(int productId)
        {
            var cart = GetCart();
            cart.Items.RemoveAll(i => i.ProductId == productId);
            SaveCart(cart);
        }

        public void ClearCart()
        {
            Session.Remove(CartSessionKey);
        }
    }
}
