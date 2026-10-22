using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using MiniECommerce.Models;
using System.ComponentModel.DataAnnotations;

namespace MiniECommerce.ViewModels
{
    public class CategorySummaryViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int ProductCount { get; set; }
    }

    public class HomeViewModel
    {
        public IReadOnlyList<Product> LatestProducts { get; set; } = Array.Empty<Product>();
        public IReadOnlyList<CategorySummaryViewModel> Categories { get; set; } = Array.Empty<CategorySummaryViewModel>();
    }

    public class ProductDetailsViewModel
    {
        public Product Product { get; set; } = null!;
        public IReadOnlyList<Product> Related { get; set; } = Array.Empty<Product>();
    }

    /// <summary>Generic pager. Links are real URLs (works without JS); JS upgrades them to AJAX.</summary>
    public class PagerViewModel
    {
        public int PageNumber { get; set; } = 1;
        public int TotalPages { get; set; } = 1;
        public string Action { get; set; } = "Index";
        public string Controller { get; set; } = string.Empty;
        public Dictionary<string, object?> RouteValues { get; set; } = new();
    }

    /// <summary>
    /// Used for creating/editing products. Binding a dedicated view model (instead of the
    /// EF entity) prevents over-posting: the browser can only set the fields listed here.
    /// </summary>
    public class ProductFormViewModel : IValidatableObject
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, ErrorMessage = "Name must be at most 100 characters long")]
        public string Name { get; set; } = string.Empty;

        [StringLength(2000, ErrorMessage = "Description must be at most 2000 characters long")]
        public string? Description { get; set; }

        [Range(0.01, 1000000, ErrorMessage = "Price must be greater than 0")]
        public decimal Price { get; set; }

        [Range(0, 100000, ErrorMessage = "Stock must be a non-negative integer")]
        public int Stock { get; set; }

        [Display(Name = "Image URL")]
        [StringLength(500, ErrorMessage = "Image URL must be at most 500 characters long")]
        public string? ImageUrl { get; set; }

        [Display(Name = "Product Image")]
        public IFormFile? ImageFile { get; set; }

        [Display(Name = "Category")]
        [Range(1, int.MaxValue, ErrorMessage = "Please choose a category")]
        public int CategoryId { get; set; }

        [Display(Name = "Visible in the store")]
        public bool IsActive { get; set; } = true;

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (string.IsNullOrWhiteSpace(ImageUrl))
                yield break;

            var url = ImageUrl.Trim();
            var isLocalPath = url.StartsWith('/') && !url.StartsWith("//");
            var isAbsoluteHttp = Uri.TryCreate(url, UriKind.Absolute, out var uri)
                                 && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);

            if (!isLocalPath && !isAbsoluteHttp)
            {
                yield return new ValidationResult(
                    "Image URL must start with http:// or https://, or be a local path such as /images/products/item.jpg",
                    new[] { nameof(ImageUrl) });
            }
        }
    }

    public class CategoryFormViewModel
    {
        public int? Id { get; set; }

        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, ErrorMessage = "Name must be at most 100 characters long")]
        public string Name { get; set; } = string.Empty;

        [StringLength(250, ErrorMessage = "Description must be at most 250 characters long")]
        public string? Description { get; set; }
    }

    public class AdminOrderListViewModel
    {
        public IReadOnlyList<Order> Orders { get; set; } = Array.Empty<Order>();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; } = 1;
        public int TotalPages { get; set; } = 1;
        public OrderStatus? Status { get; set; }
        public string? Search { get; set; }
    }

    public class AdminDashboardViewModel
    {
        public int TotalProducts { get; set; }
        public int TotalCategories { get; set; }
        public int TotalOrders { get; set; }
        public int PendingOrders { get; set; }
        public int LowStockProducts { get; set; }
        public IReadOnlyList<Order> RecentOrders { get; set; } = Array.Empty<Order>();
    }
}
