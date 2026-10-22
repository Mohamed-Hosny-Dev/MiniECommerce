using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniECommerce.Services.Interfaces;

namespace MiniECommerce.Controllers
{
    // Anti-forgery validation for every POST is enforced globally (AutoValidateAntiforgeryToken in Program.cs).
    // The AJAX code sends the token in the "RequestVerificationToken" header.
    [Authorize]
    public class CartController : Controller
    {
        private readonly ICartService _cartService;

        public CartController(ICartService cartService)
        {
            _cartService = cartService;
        }

        public async Task<IActionResult> Index()
        {
            // Refresh prices / stock first so the customer never sees stale numbers.
            var sync = await _cartService.SyncCartAsync();
            ViewBag.Notices = sync.Notices;
            return View(sync.Cart);
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart(int productId, int quantity = 1)
        {
            var result = await _cartService.AddToCartAsync(productId, quantity);
            var cart = _cartService.GetCart();

            return Json(new
            {
                success = result.Success,
                message = result.Message,
                cartCount = cart.ItemCount
            });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateQuantity(int productId, int quantity)
        {
            var result = await _cartService.UpdateQuantityAsync(productId, quantity);
            var cart = _cartService.GetCart();
            var item = cart.Items.FirstOrDefault(i => i.ProductId == productId);

            return Json(new
            {
                success = result.Success,          // used to be hard-coded to true, hiding stock errors
                message = result.Message,
                removed = item == null,
                quantity = item?.Quantity ?? 0,    // server truth, so the UI can revert a rejected change
                itemSubtotal = item?.Subtotal ?? 0,
                cartTotal = cart.Total,
                cartCount = cart.ItemCount
            });
        }

        [HttpPost]
        public IActionResult RemoveFromCart(int productId)
        {
            _cartService.RemoveFromCart(productId);
            var cart = _cartService.GetCart();

            return Json(new
            {
                success = true,
                cartTotal = cart.Total,
                cartCount = cart.ItemCount
            });
        }
    }
}
