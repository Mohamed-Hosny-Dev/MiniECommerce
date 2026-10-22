using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiniECommerce.Models
{
    public class Order
    {
        public int Id { get; set; }
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        [Column(TypeName ="decimal(18,2)")]
        public decimal TotalAmount { get; set; }
        public OrderStatus Status { get; set; } = OrderStatus.Pending;
        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.CashOnDelivery;

        [Required(ErrorMessage = "Customer name is required")]
        public string CustomerName { get; set; } = string.Empty;
        [Required(ErrorMessage = "Customer Phone is required")]
        [Phone]
        public string CustomerPhone { get; set; } = string.Empty;
        [Required(ErrorMessage = "Customer Governorate is required")]
        public string Governorate { get; set; } = string.Empty;

        [Required(ErrorMessage = "Customer City is required")]
        public string City { get; set; } = string.Empty;

        [Required(ErrorMessage = "Customer Address is required")]
        public string Address { get; set; } = string.Empty;
        
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser? User { get; set; }

        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    }
}
