using MiniECommerce.ViewModels;

namespace MiniECommerce.Services.Interfaces
{
    public interface ICartService
    {
        CartViewModel GetCart();

        /// <summary>Re-checks the cart against the database (prices, availability, stock) and fixes it.</summary>
        Task<CartSyncResult> SyncCartAsync();

        Task<(bool Success, string Message)> AddToCartAsync(int productId, int quantity);
        Task<(bool Success, string Message)> UpdateQuantityAsync(int productId, int quantity);
        void RemoveFromCart(int productId);
        void ClearCart();
    }
}
