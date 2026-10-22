namespace MiniECommerce.ViewModels
{
    public class CartItemViewModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public decimal Subtotal => UnitPrice * Quantity;
    }

    public class CartViewModel
    {
        /// <summary>Upper limit for a single product line (also protects against int overflow tricks).</summary>
        public const int MaxQuantityPerItem = 20;

        public List<CartItemViewModel> Items { get; set; } = new List<CartItemViewModel>();
        public decimal Total => Items.Sum(i => i.Subtotal);
        public int ItemCount => Items.Sum(i => i.Quantity);
    }

    /// <summary>Cart re-validated against the database, plus human-readable notices about what changed.</summary>
    public class CartSyncResult
    {
        public CartViewModel Cart { get; set; } = new CartViewModel();
        public List<string> Notices { get; set; } = new List<string>();
    }
}
