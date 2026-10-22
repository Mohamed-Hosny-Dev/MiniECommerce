using MiniECommerce.Models;
using MiniECommerce.Repository.Interfaces;
using MiniECommerce.Services.Interfaces;
using MiniECommerce.ViewModels;

namespace MiniECommerce.Services.Implementation
{
    public class CategoryService : ICategoryService
    {
        private readonly IUnitOfWork _unitOfWork;

        public CategoryService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<Category>> GetAllCategoriesAsync()
        {
            return await _unitOfWork.Categories.FindAsync(
                c => true,
                null,
                q => q.OrderBy(c => c.Name));
        }

        public async Task<IReadOnlyList<CategorySummaryViewModel>> GetAdminSummariesAsync()
        {
            return await _unitOfWork.Categories.SelectAsync(
                c => new CategorySummaryViewModel
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    ProductCount = c.Products.Count()
                },
                null,
                q => q.OrderBy(c => c.Name));
        }

        public async Task<Category?> GetByIdAsync(int id)
        {
            return await _unitOfWork.Categories.GetByIdAsync(id);
        }

        public async Task<(bool Success, string Message)> SaveCategoryAsync(CategoryFormViewModel model)
        {
            var name = model.Name.Trim();
            var description = string.IsNullOrWhiteSpace(model.Description) ? null : model.Description.Trim();
            var id = model.Id ?? 0;

            var duplicate = await _unitOfWork.Categories.CountAsync(c => c.Name == name && c.Id != id);
            if (duplicate > 0)
                return (false, "A category with this name already exists.");

            if (model.Id.HasValue)
            {
                var category = await _unitOfWork.Categories.GetByIdAsync(model.Id.Value);
                if (category == null)
                    return (false, "Category not found.");

                category.Name = name;
                category.Description = description;
                _unitOfWork.Categories.Update(category);
                await _unitOfWork.SaveChangesAsync();
                return (true, "Category updated.");
            }

            await _unitOfWork.Categories.AddAsync(new Category { Name = name, Description = description });
            await _unitOfWork.SaveChangesAsync();
            return (true, "Category created.");
        }

        public async Task<(bool Success, string Message)> DeleteCategoryAsync(int id)
        {
            var category = await _unitOfWork.Categories.GetByIdAsync(id);
            if (category == null)
                return (false, "Category not found.");

            // The FK is configured as Restrict, so check first and give a friendly message.
            var productCount = await _unitOfWork.Products.CountAsync(p => p.CategoryId == id);
            if (productCount > 0)
                return (false, $"This category still has {productCount} product(s). Move or delete them first.");

            _unitOfWork.Categories.Delete(category);
            await _unitOfWork.SaveChangesAsync();
            return (true, "Category deleted.");
        }
    }
}
