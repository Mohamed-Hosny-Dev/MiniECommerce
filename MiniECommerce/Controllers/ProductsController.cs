using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MiniECommerce.Extensions;
using MiniECommerce.Services.Interfaces;
using MiniECommerce.ViewModels;
using Microsoft.AspNetCore.Hosting;
using System.IO;
namespace MiniECommerce.Controllers
{
    public class ProductsController : Controller
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProductsController(IProductService productService, ICategoryService categoryService, IWebHostEnvironment webHostEnvironment)
        {
            _productService = productService;
            _categoryService = categoryService;
            _webHostEnvironment = webHostEnvironment;
        }

        // ==========================================
        // Customer Actions
        // ==========================================

        // 1. Product list with search, category filter, sorting and pagination (AJAX aware).
        public async Task<IActionResult> Index(string? search, int? categoryId, int pageNumber = 1, string? sort = null)
        {
            var viewModel = await _productService.GetFilteredProductAsync(search, categoryId, pageNumber, 8, sort);

            if (Request.IsAjaxRequest())
            {
                Response.MarkAsAjaxFragment();
                return PartialView("_ProductListPartial", viewModel);
            }

            return View(viewModel);
        }

        // 2. Product details (+ a few related products).
        public async Task<IActionResult> Details(int id)
        {
            var product = await _productService.GetByIdAsync(id);
            if (product == null || !product.IsActive)
            {
                return NotFound();
            }

            var related = await _productService.GetFilteredProductAsync(null, product.CategoryId, 1, 5);

            return View(new ProductDetailsViewModel
            {
                Product = product,
                Related = related.Products.Where(p => p.Id != product.Id).Take(4).ToList()
            });
        }

        // ==========================================
        // Admin Actions
        // ==========================================

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create()
        {
            await PopulateCategoriesAsync();
            return View(new ProductFormViewModel());
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(ProductFormViewModel model)
        {
            await ValidateCategoryAsync(model);

            if (!ModelState.IsValid)
            {
                await PopulateCategoriesAsync(model.CategoryId);
                return View(model);
            }

            model.ImageUrl = await ProcessUploadedFileAsync(model);

            await _productService.CreateProductAsync(model);
            TempData["SuccessMessage"] = $"\"{model.Name}\" was created.";
            return RedirectToAction("Products", "Admin");
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            var model = await _productService.GetFormModelAsync(id);
            if (model == null)
            {
                return NotFound();
            }

            await PopulateCategoriesAsync(model.CategoryId);
            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, ProductFormViewModel model)
        {
            model.Id = id; // the URL is the only source of truth for which product is edited

            await ValidateCategoryAsync(model);

            if (!ModelState.IsValid)
            {
                await PopulateCategoriesAsync(model.CategoryId);
                return View(model);
            }
            if (model.ImageFile != null)
            {
                model.ImageUrl = await ProcessUploadedFileAsync(model);
            }
            if (!await _productService.UpdateProductAsync(model))
            {
                return NotFound();
            }

            TempData["SuccessMessage"] = $"\"{model.Name}\" was updated.";
            return RedirectToAction("Products", "Admin");
        }


        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var success = await _productService.DeleteProductAsync(id);

                if (Request.IsAjaxRequest())
                {
                    return Json(success
                        ? new { success = true, message = "Product deleted successfully." }
                        : new { success = false, message = "Product not found." });
                }

                return RedirectToAction("Products", "Admin");
            }
            catch (Exception)
            {
                if (Request.IsAjaxRequest())
                {
                    return Json(new { success = false, message = "Cannot delete this product because it is part of existing orders. Try hiding it instead." });
                }
                return RedirectToAction("Products", "Admin");
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ToggleActiveStatus(int id)
        {
            var isActive = await _productService.ToggleActiveStatusAsync(id);

            if (Request.IsAjaxRequest())
            {
                return Json(isActive.HasValue
                    ? new { success = true, isActive = isActive.Value, message = isActive.Value ? "Product is now visible in the store." : "Product is now hidden from the store." }
                    : new { success = false, isActive = false, message = "Product not found." });
            }

            return RedirectToAction("Products", "Admin");
        }

        private async Task PopulateCategoriesAsync(int? selectedId = null)
        {
            var categories = await _categoryService.GetAllCategoriesAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name", selectedId);
        }

        private async Task ValidateCategoryAsync(ProductFormViewModel model)
        {
            if (model.CategoryId > 0 && await _categoryService.GetByIdAsync(model.CategoryId) == null)
            {
                ModelState.AddModelError(nameof(model.CategoryId), "The selected category does not exist.");
            }
        }
        private async Task<string?> ProcessUploadedFileAsync(ProductFormViewModel model)
        {
            if (model.ImageFile != null)
            {
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "products");

                Directory.CreateDirectory(uploadsFolder);

                string uniqueFileName = Guid.NewGuid().ToString() + "_" + model.ImageFile.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await model.ImageFile.CopyToAsync(fileStream);
                }

                return "/images/products/" + uniqueFileName;
            }

            return model.ImageUrl;
        }
    }
}
