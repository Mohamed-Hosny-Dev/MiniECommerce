using MiniECommerce.Models;
using MiniECommerce.ViewModels;

namespace MiniECommerce.Services.Interfaces
{
    public interface IProductService
    {
        Task<ProductListViewModel> GetFilteredProductAsync(
            string? search,
            int? categoryId,
            int pageNumber = 1,
            int pageSize = 8,
            string? sort = null,
            bool includeInactive = false);

        Task<IReadOnlyList<Product>> GetLatestProductsAsync(int count);
        Task<IReadOnlyList<CategorySummaryViewModel>> GetCategorySummariesAsync();
        Task<Product?> GetByIdAsync(int id);
        Task<ProductFormViewModel?> GetFormModelAsync(int id);
        Task<int> CreateProductAsync(ProductFormViewModel model);
        Task<bool> UpdateProductAsync(ProductFormViewModel model);

        /// <returns>The new IsActive value, or null if the product doesn't exist.</returns>
        Task<bool?> ToggleActiveStatusAsync(int id);
        Task<bool> DeleteProductAsync(int id);
    }
}
