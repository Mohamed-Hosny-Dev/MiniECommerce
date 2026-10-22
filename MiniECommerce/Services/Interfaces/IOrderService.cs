using MiniECommerce.Models;
using MiniECommerce.ViewModels;

namespace MiniECommerce.Services.Interfaces
{
    public interface IOrderService
    {
        Task<(bool Success, string Message, int OrderId)> CreateOrderAsync(CheckoutViewModel model, string userId);
        Task<IEnumerable<Order>> GetUserOrdersAsync(string userId);
        Task<Order?> GetOrderDetailsAsync(int orderId, string userId);
        Task<bool> CancelOrderAsync(int orderId, string userId);

        // Admin
        Task<AdminOrderListViewModel> GetAdminOrdersAsync(OrderStatus? status, string? search, int pageNumber = 1, int pageSize = 20);
        Task<(bool Success, string Message)> UpdateOrderStatusAsync(int orderId, OrderStatus newStatus);
    }
}
