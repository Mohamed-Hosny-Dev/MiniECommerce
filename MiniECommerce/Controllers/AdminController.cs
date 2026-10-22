using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniECommerce.Extensions;
using MiniECommerce.Models;
using MiniECommerce.Repository.Interfaces;
using MiniECommerce.Services;
using MiniECommerce.Services.Interfaces;
using MiniECommerce.ViewModels;

namespace MiniECommerce.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private const int LowStockThreshold = 5;

        private readonly IUnitOfWork _unitOfWork;
        private readonly IOrderService _orderService;
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;

        public AdminController(
            IUnitOfWork unitOfWork,
            IOrderService orderService,
            IProductService productService,
            ICategoryService categoryService)
        {
            _unitOfWork = unitOfWork;
            _orderService = orderService;
            _productService = productService;
            _categoryService = categoryService;
        }

        public async Task<IActionResult> Index()
        {
            var recent = await _orderService.GetAdminOrdersAsync(null, null, 1, 5);

            return View(new AdminDashboardViewModel
            {
                TotalProducts = await _unitOfWork.Products.CountAsync(),
                TotalCategories = await _unitOfWork.Categories.CountAsync(),
                TotalOrders = await _unitOfWork.Orders.CountAsync(),
                PendingOrders = await _unitOfWork.Orders.CountAsync(o => o.Status == OrderStatus.Pending),
                LowStockProducts = await _unitOfWork.Products.CountAsync(p => p.IsActive && p.Stock <= LowStockThreshold),
                RecentOrders = recent.Orders
            });
        }

        // ---------------- Orders ----------------

        public async Task<IActionResult> Orders(OrderStatus? status, string? search, int pageNumber = 1)
        {
            var model = await _orderService.GetAdminOrdersAsync(status, search, pageNumber);

            if (Request.IsAjaxRequest())
            {
                Response.MarkAsAjaxFragment();
                return PartialView("_OrderListPartial", model);
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateOrderStatus(int orderId, OrderStatus status)
        {
            // Model binding happily accepts any number for an enum (e.g. 99), so check it explicitly.
            if (!Enum.IsDefined(status))
            {
                return Json(new { success = false, message = "Invalid order status." });
            }

            var result = await _orderService.UpdateOrderStatusAsync(orderId, status);
            return Json(new { success = result.Success, message = result.Message });
        }

        // ---------------- Products ----------------

        // The public catalogue hides inactive products, so the admin needs this list to find (and re-activate) them.
        public async Task<IActionResult> Products(string? search, int pageNumber = 1)
        {
            var model = await _productService.GetFilteredProductAsync(search, null, pageNumber, 12, "newest", includeInactive: true);

            if (Request.IsAjaxRequest())
            {
                Response.MarkAsAjaxFragment();
                return PartialView("_AdminProductTablePartial", model);
            }

            return View(model);
        }

        // ---------------- Categories ----------------

        public async Task<IActionResult> Categories()
        {
            return View(await _categoryService.GetAdminSummariesAsync());
        }

        [HttpPost]
        public async Task<IActionResult> SaveCategory(CategoryFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .Where(m => !string.IsNullOrWhiteSpace(m));

                return Json(new { success = false, message = string.Join(" ", errors) });
            }

            var result = await _categoryService.SaveCategoryAsync(model);
            return Json(new { success = result.Success, message = result.Message });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var result = await _categoryService.DeleteCategoryAsync(id);
            return Json(new { success = result.Success, message = result.Message });
        }
    }
}
