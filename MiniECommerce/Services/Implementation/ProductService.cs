using MiniECommerce.Models;
using MiniECommerce.Repository.Interfaces;
using MiniECommerce.Services.Interfaces;
using MiniECommerce.ViewModels;

namespace MiniECommerce.Services.Implementation
{
    public class ProductService : IProductService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ProductService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ProductListViewModel> GetFilteredProductAsync(
            string? search,
            int? categoryId,
            int pageNumber = 1,
            int pageSize = 8,
            string? sort = null,
            bool includeInactive = false)
        {
            search = string.IsNullOrWhiteSpace(search) ? null : search.Trim();
            pageSize = Math.Clamp(pageSize, 1, 50);

            // Filtering, sorting and paging all happen in SQL (the old version loaded every product into memory).
            var page = await _unitOfWork.Products.GetPagedAsync(
                p => (includeInactive || p.IsActive) &&
                     (search == null || p.Name.Contains(search) || (p.Description != null && p.Description.Contains(search))) &&
                     (!categoryId.HasValue || p.CategoryId == categoryId.Value),
                pageNumber,
                pageSize,
                BuildOrdering(sort),
                new[] { "Category" });

            var categories = await _unitOfWork.Categories.FindAsync(
                c => true,
                null,
                q => q.OrderBy(c => c.Name));

            return new ProductListViewModel
            {
                Products = page.Items,
                Categories = categories,
                SearchQuery = search,
                SelectedCategoryId = categoryId,
                Sort = sort,
                PageNumber = page.PageNumber,
                TotalPages = page.TotalPages,
                TotalCount = page.TotalCount
            };
        }

        private static Func<IQueryable<Product>, IOrderedQueryable<Product>> BuildOrdering(string? sort) => sort switch
        {
            "price_asc" => q => q.OrderBy(p => p.Price).ThenByDescending(p => p.Id),
            "price_desc" => q => q.OrderByDescending(p => p.Price).ThenByDescending(p => p.Id),
            "name" => q => q.OrderBy(p => p.Name).ThenBy(p => p.Id),
            _ => q => q.OrderByDescending(p => p.Id) // newest first
        };

        public async Task<IReadOnlyList<Product>> GetLatestProductsAsync(int count)
        {
            var page = await _unitOfWork.Products.GetPagedAsync(
                p => p.IsActive,
                1,
                Math.Clamp(count, 1, 50),
                q => q.OrderByDescending(p => p.Id),
                new[] { "Category" });

            return page.Items;
        }

        public async Task<IReadOnlyList<CategorySummaryViewModel>> GetCategorySummariesAsync()
        {
            return await _unitOfWork.Categories.SelectAsync(
                c => new CategorySummaryViewModel
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    ProductCount = c.Products.Count(p => p.IsActive)
                },
                null,
                q => q.OrderBy(c => c.Name));
        }

        public async Task<Product?> GetByIdAsync(int id)
        {
            return await _unitOfWork.Products.GetFirstOrDefaultAsync(
                p => p.Id == id,
                new[] { "Category" });
        }

        public async Task<ProductFormViewModel?> GetFormModelAsync(int id)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(id);
            if (product == null)
                return null;

            return new ProductFormViewModel
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                Stock = product.Stock,
                ImageUrl = product.ImageUrl == "default-product.png" ? null : product.ImageUrl,
                CategoryId = product.CategoryId,
                IsActive = product.IsActive
            };
        }

        public async Task<int> CreateProductAsync(ProductFormViewModel model)
        {
            var product = new Product();
            Apply(model, product);

            await _unitOfWork.Products.AddAsync(product);
            await _unitOfWork.SaveChangesAsync();
            return product.Id;
        }

        public async Task<bool> UpdateProductAsync(ProductFormViewModel model)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(model.Id);
            if (product == null)
                return false;

            // Only the whitelisted fields are copied, so nothing else on the entity can be tampered with.
            Apply(model, product);
            _unitOfWork.Products.Update(product);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<bool?> ToggleActiveStatusAsync(int id)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(id);
            if (product == null)
                return null;

            product.IsActive = !product.IsActive;
            _unitOfWork.Products.Update(product);
            await _unitOfWork.SaveChangesAsync();
            return product.IsActive;
        }

        private static void Apply(ProductFormViewModel model, Product product)
        {
            product.Name = model.Name.Trim();
            product.Description = string.IsNullOrWhiteSpace(model.Description) ? null : model.Description.Trim();
            product.Price = model.Price;
            product.Stock = model.Stock;
            product.ImageUrl = string.IsNullOrWhiteSpace(model.ImageUrl) ? "default-product.png" : model.ImageUrl.Trim();
            product.CategoryId = model.CategoryId;
            product.IsActive = model.IsActive;
        }
        public async Task<bool> DeleteProductAsync(int id)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(id);
            if (product == null)
            {
                return false;
            }

            _unitOfWork.Products.Delete(product);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }
    }
}
