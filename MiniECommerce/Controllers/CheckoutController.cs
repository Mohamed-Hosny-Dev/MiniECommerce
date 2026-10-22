using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MiniECommerce.Models;
using MiniECommerce.Services.Interfaces;
using MiniECommerce.ViewModels;
using System.Security.Claims;

namespace MiniECommerce.Controllers
{
    [Authorize]
    public class CheckoutController : Controller
    {
        private readonly IOrderService _orderService;
        private readonly ICartService _cartService;
        private readonly UserManager<ApplicationUser> _userManager;

        public CheckoutController(
            IOrderService orderService,
            ICartService cartService,
            UserManager<ApplicationUser> userManager)
        {
            _orderService = orderService;
            _cartService = cartService;
            _userManager = userManager;
        }

        [HttpGet]
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var sync = await _cartService.SyncCartAsync();

            if (!sync.Cart.Items.Any())
            {
                TempData["ErrorMessage"] = sync.Notices.FirstOrDefault() ?? "Your cart is empty.";
                return RedirectToAction("Index", "Cart");
            }

            var user = await _userManager.GetUserAsync(User);

            var fullName = $"{user?.FirstName} {user?.LastName}".Trim();
            if (string.IsNullOrWhiteSpace(fullName))
            {
                fullName = user?.UserName ?? string.Empty;
            }

            ViewBag.Notices = sync.Notices;

            return View(new CheckoutViewModel
            {
                CustomerName = fullName,
                Phone = user?.PhoneNumber ?? string.Empty,
                Address = user?.Address ?? string.Empty,
                Cart = sync.Cart
            });
        }

        [HttpPost]
        public async Task<IActionResult> Index(CheckoutViewModel model)
        {
            // Always rebuild the cart from the session + database, never from the posted form.
            var sync = await _cartService.SyncCartAsync();
            model.Cart = sync.Cart;

            if (!model.Cart.Items.Any())
            {
                TempData["ErrorMessage"] = sync.Notices.FirstOrDefault() ?? "Your cart is empty.";
                return RedirectToAction("Index", "Cart");
            }

            // If a price/stock changed while the customer was filling the form, show it before charging them.
            if (sync.Notices.Count > 0)
            {
                ViewBag.Notices = sync.Notices;
                ModelState.Clear();
                return View(model);
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }

            var result = await _orderService.CreateOrderAsync(model, userId);

            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.Message);
                return View(model);
            }

            return RedirectToAction(nameof(Confirmation), new { id = result.OrderId });
        }

        [HttpGet]
        public async Task<IActionResult> Confirmation(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }

            var order = await _orderService.GetOrderDetailsAsync(id, userId);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }
    }
}
