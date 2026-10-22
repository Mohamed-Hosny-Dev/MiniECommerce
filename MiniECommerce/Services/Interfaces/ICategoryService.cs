using MiniECommerce.Models;
using MiniECommerce.ViewModels;

namespace MiniECommerce.Services.Interfaces
{
    public interface ICategoryService
    {
        Task<IEnumerable<Category>> GetAllCategoriesAsync();
        Task<IReadOnlyList<CategorySummaryViewModel>> GetAdminSummariesAsync();
        Task<Category?> GetByIdAsync(int id);
        Task<(bool Success, string Message)> SaveCategoryAsync(CategoryFormViewModel model);
        Task<(bool Success, string Message)> DeleteCategoryAsync(int id);
    }
}
