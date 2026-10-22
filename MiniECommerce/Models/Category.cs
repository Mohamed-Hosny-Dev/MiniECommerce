using System.ComponentModel.DataAnnotations;

namespace MiniECommerce.Models
{
    public class Category
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, ErrorMessage = "Name must be at most 100 characters long")]
        public string Name { get; set; } = string.Empty;
        [StringLength(250, ErrorMessage = "Description must be at most 250 characters long")]
        public string? Description { get; set; }
        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
