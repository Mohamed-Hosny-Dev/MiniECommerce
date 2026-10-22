using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiniECommerce.Models
{
    public class Product
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, ErrorMessage = "Name must be at most 100 characters long")]
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }
        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Stock must be a non-negative integer")]
        public int Stock { get; set; }
        public string ImageUrl { get; set; } = "default-product.png";
        public bool IsActive { get; set; } = true;
        [Required(ErrorMessage = "Category is required")]
        public int CategoryId { get; set; }
        public Category? Category { get; set; }
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    }
}
