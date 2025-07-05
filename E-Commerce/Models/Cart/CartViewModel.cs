
namespace E_Commerce.Models
{
    public class CartViewModel
    {
        public Guid Id { get; set; }
        public List<CartItemViewModel> Items { get; set; } = new();
        public decimal TotalPrice => Items.Sum(i => i.Total);
    }
}
