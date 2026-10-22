using Microsoft.AspNetCore.Mvc;
using MiniECommerce.Models;
using MiniECommerce.Services.Interfaces;
using MiniECommerce.ViewModels;
using System.Diagnostics;

namespace MiniECommerce.Controllers
{
    public class HomeController : Controller
    {
        private readonly IProductService _productService;

        public HomeController(IProductService productService)
        {
            _productService = productService;
        }

        public async Task<IActionResult> Index()
        {
            var viewModel = new HomeViewModel
            {
                LatestProducts = await _productService.GetLatestProductsAsync(8),
                Categories = await _productService.GetCategorySummariesAsync()
            };

            return View(viewModel);
        }

        // Target of app.UseExceptionHandler("/Home/Error") - this action was missing, so a
        // production error used to produce an empty 500 page.
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }

        // Target of app.UseStatusCodePagesWithReExecute (404, 403, ...)
        [Route("Home/Status")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Status(int code = 404)
        {
            ViewBag.Code = code;
            return View();
        }
    }
}
