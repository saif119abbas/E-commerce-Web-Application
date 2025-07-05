namespace E_Commerce.Models
{

        public class CartItemViewModel
        {
            public Guid ProductId { get; set; }
            public string? ProductName { get; set; }
            public int Quantity { get; set; }
            public decimal Price { get; set; }
            public decimal Total => Quantity * Price;
        }
}
