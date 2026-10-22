using MiniECommerce.Models;

namespace MiniECommerce.ViewModels
{
    public class ProductListViewModel
    {
        public IEnumerable<Product> Products { get; set; } = new List<Product>();
        public IEnumerable<Category> Categories { get; set; } = new List<Category>();

        public string? SearchQuery { get; set; }
        public int? SelectedCategoryId { get; set; }
        public string? Sort { get; set; }

        public int PageNumber { get; set; } = 1;
        public int TotalPages { get; set; }
        public int TotalCount { get; set; }
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;
    }
}
