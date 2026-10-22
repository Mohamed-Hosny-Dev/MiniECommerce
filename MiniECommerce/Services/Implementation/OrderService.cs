using Microsoft.EntityFrameworkCore;
using MiniECommerce.Data;
using MiniECommerce.Models;
using MiniECommerce.Repository.Interfaces;
using MiniECommerce.Services.Interfaces;
using MiniECommerce.ViewModels;

namespace MiniECommerce.Services.Implementation
{
    public class OrderService : IOrderService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICartService _cartService;
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<OrderService> _logger;

        public OrderService(
            IUnitOfWork unitOfWork,
            ICartService cartService,
            ApplicationDbContext dbContext,
            ILogger<OrderService> logger)
        {
            _unitOfWork = unitOfWork;
            _cartService = cartService;
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<(bool Success, string Message, int OrderId)> CreateOrderAsync(
            CheckoutViewModel model,
            string userId)
        {
            var cart = _cartService.GetCart();

            // 1. Check if the cart is empty
            if (cart.Items == null || !cart.Items.Any())
                return (false, "Your cart is empty.", 0);

            // 2. Everything below is one atomic unit: stock + order succeed together or not at all
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                var orderItems = new List<OrderItem>();
                decimal totalAmount = 0;

                foreach (var item in cart.Items)
                {
                    var productId = item.ProductId;
                    var quantity = item.Quantity;

                    if (quantity < 1 || quantity > CartViewModel.MaxQuantityPerItem)
                    {
                        await transaction.RollbackAsync();
                        return (false, "Invalid product quantity.", 0);
                    }

                    var product = await _dbContext.Products
                        .AsNoTracking()
                        .FirstOrDefaultAsync(p => p.Id == productId);

                    if (product == null || !product.IsActive)
                    {
                        await transaction.RollbackAsync();
                        return (false, $"Product '{item.ProductName}' is not available.", 0);
                    }

                    // 3. Take the stock with ONE atomic conditional UPDATE.
                    //    "read stock, check, write stock" lets two simultaneous customers both buy the last
                    //    item; this statement succeeds for only one of them (the other gets 0 rows affected).
                    var affected = await _dbContext.Products
                        .Where(p => p.Id == productId && p.IsActive && p.Stock >= quantity)
                        .ExecuteUpdateAsync(s => s.SetProperty(p => p.Stock, p => p.Stock - quantity));

                    if (affected == 0)
                    {
                        await transaction.RollbackAsync();
                        return (false, $"Not enough stock for '{product.Name}'. Please review your cart.", 0);
                    }

                    // Prices always come from the database, never from the browser/session.
                    totalAmount += product.Price * quantity;
                    orderItems.Add(new OrderItem
                    {
                        ProductId = product.Id,
                        Quantity = quantity,
                        UnitPrice = product.Price
                    });
                }

                // 4. Create the order
                var order = new Order
                {
                    UserId = userId,
                    CustomerName = model.CustomerName.Trim(),
                    CustomerPhone = model.Phone.Trim(),
                    Governorate = model.Governorate,
                    City = model.City.Trim(),
                    Address = model.Address.Trim(),
                    TotalAmount = totalAmount,
                    Status = OrderStatus.Pending,
                    PaymentMethod = model.PaymentMethod,
                    OrderDate = DateTime.UtcNow,
                    OrderItems = orderItems
                };

                await _unitOfWork.Orders.AddAsync(order);
                await _unitOfWork.SaveChangesAsync();

                await transaction.CommitAsync();

                // 5. Clear the cart only after the order is safely stored
                _cartService.ClearCart();

                return (true, "Order created successfully.", order.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create an order for user {UserId}", userId);

                try
                {
                    await transaction.RollbackAsync();
                }
                catch (Exception rollbackException)
                {
                    _logger.LogError(rollbackException, "Rollback failed for user {UserId}", userId);
                }

                return (false, "An error occurred while processing your order. Please try again.", 0);
            }
        }

        public async Task<IEnumerable<Order>> GetUserOrdersAsync(string userId)
        {
            return await _unitOfWork.Orders.FindAsync(
                o => o.UserId == userId,
                null,
                q => q.OrderByDescending(o => o.OrderDate).ThenByDescending(o => o.Id));
        }

        public async Task<Order?> GetOrderDetailsAsync(int orderId, string userId)
        {
            return await _unitOfWork.Orders.GetFirstOrDefaultAsync(
                o => o.Id == orderId && o.UserId == userId,
                new[] { "OrderItems.Product" });
        }

        public async Task<bool> CancelOrderAsync(int orderId, string userId)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                // Flip Pending -> Cancelled atomically, so a double click or two tabs
                // can never restore the stock twice.
                var updated = await _dbContext.Orders
                    .Where(o => o.Id == orderId && o.UserId == userId && o.Status == OrderStatus.Pending)
                    .ExecuteUpdateAsync(s => s.SetProperty(o => o.Status, OrderStatus.Cancelled));

                if (updated == 0)
                {
                    await transaction.RollbackAsync();
                    return false;
                }

                await RestoreStockAsync(orderId);
                await transaction.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to cancel order {OrderId}", orderId);
                await transaction.RollbackAsync();
                return false;
            }
        }

        public async Task<AdminOrderListViewModel> GetAdminOrdersAsync(
            OrderStatus? status,
            string? search,
            int pageNumber = 1,
            int pageSize = 20)
        {
            search = string.IsNullOrWhiteSpace(search) ? null : search.Trim();
            int? orderId = int.TryParse(search?.TrimStart('#'), out var parsedId) ? parsedId : null;

            var page = await _unitOfWork.Orders.GetPagedAsync(
                o => (!status.HasValue || o.Status == status.Value) &&
                     (search == null ||
                      o.CustomerName.Contains(search) ||
                      o.CustomerPhone.Contains(search) ||
                      (orderId.HasValue && o.Id == orderId.Value)),
                pageNumber,
                pageSize,
                q => q.OrderByDescending(o => o.OrderDate).ThenByDescending(o => o.Id));

            return new AdminOrderListViewModel
            {
                Orders = page.Items,
                TotalCount = page.TotalCount,
                PageNumber = page.PageNumber,
                TotalPages = page.TotalPages,
                Status = status,
                Search = search
            };
        }

        public async Task<(bool Success, string Message)> UpdateOrderStatusAsync(int orderId, OrderStatus newStatus)
        {
            var currentStatus = await _dbContext.Orders
                .AsNoTracking()
                .Where(o => o.Id == orderId)
                .Select(o => (OrderStatus?)o.Status)
                .FirstOrDefaultAsync();

            if (currentStatus == null)
                return (false, "Order not found.");

            var current = currentStatus.Value;

            if (current == newStatus)
                return (true, "Status is unchanged.");

            if (!OrderStatusRules.CanTransition(current, newStatus))
                return (false, $"An order that is {current} cannot be changed to {newStatus}.");

            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                // Conditional on the status we just read: protects against two admins changing the same order.
                var updated = await _dbContext.Orders
                    .Where(o => o.Id == orderId && o.Status == current)
                    .ExecuteUpdateAsync(s => s.SetProperty(o => o.Status, newStatus));

                if (updated == 0)
                {
                    await transaction.RollbackAsync();
                    return (false, "The order was changed by someone else. Please refresh the list.");
                }

                // An admin cancelling an order must give the stock back, exactly like a customer cancelling.
                if (newStatus == OrderStatus.Cancelled)
                    await RestoreStockAsync(orderId);

                await transaction.CommitAsync();
                return (true, $"Order #{orderId} is now {newStatus}.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to change the status of order {OrderId}", orderId);
                await transaction.RollbackAsync();
                return (false, "Could not update the order status.");
            }
        }

        private async Task RestoreStockAsync(int orderId)
        {
            var lines = await _dbContext.OrderItems
                .AsNoTracking()
                .Where(oi => oi.OrderId == orderId)
                .Select(oi => new { oi.ProductId, oi.Quantity })
                .ToListAsync();

            foreach (var line in lines)
            {
                var productId = line.ProductId;
                var quantity = line.Quantity;

                await _dbContext.Products
                    .Where(p => p.Id == productId)
                    .ExecuteUpdateAsync(s => s.SetProperty(p => p.Stock, p => p.Stock + quantity));
            }
        }
    }
}
