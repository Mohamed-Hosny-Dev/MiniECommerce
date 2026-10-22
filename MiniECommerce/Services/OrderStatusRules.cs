using MiniECommerce.Models;

namespace MiniECommerce.Services
{
    /// <summary>
    /// The allowed life-cycle of an order. Delivered and Cancelled are final:
    /// re-opening a cancelled order would require taking the stock back again, so it is not allowed.
    /// </summary>
    public static class OrderStatusRules
    {
        public static IReadOnlyList<OrderStatus> AllowedNext(OrderStatus current) => current switch
        {
            OrderStatus.Pending => new[] { OrderStatus.Processing, OrderStatus.Cancelled },
            OrderStatus.Processing => new[] { OrderStatus.Shipped, OrderStatus.Cancelled },
            OrderStatus.Shipped => new[] { OrderStatus.Delivered },
            _ => Array.Empty<OrderStatus>()
        };

        public static bool CanTransition(OrderStatus from, OrderStatus to) =>
            AllowedNext(from).Contains(to);
    }
}
