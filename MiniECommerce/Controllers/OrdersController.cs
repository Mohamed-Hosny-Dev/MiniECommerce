using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniECommerce.Services.Interfaces;
using System.Security.Claims;

namespace MiniECommerce.Controllers
{
    [Authorize]
    public class OrdersController : Controller
    {
        private readonly IOrderService _orderService;

        public OrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var orders = await _orderService.GetUserOrdersAsync(userId);
            return View(orders);
        }

        public async Task<IActionResult> Details(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var order = await _orderService.GetOrderDetailsAsync(id, userId);
            if (order == null)
            {
                return NotFound();
            }
            return View(order);
        }

        [HttpPost]
        public async Task<IActionResult> Cancel(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var success = await _orderService.CancelOrderAsync(id, userId);
            if (!success)
            {
                TempData["ErrorMessage"] = "Unable to cancel the order. Only pending orders can be cancelled.";
                return RedirectToAction(nameof(Details), new { id });
            }

            TempData["SuccessMessage"] = "Order cancelled successfully.";
            return RedirectToAction(nameof(Details), new { id });
        }
    }
}
