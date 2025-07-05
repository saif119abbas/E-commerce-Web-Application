namespace E_Commerce.Models
{
    public class ProductItem
    {
        public Guid Id { get; set; }
        public Guid VendorId { get; set; }
        public string ? VendorName { get; set; }
        public string? Name { get; set; }
        public decimal? Price { get; set; }
        public string? CategoryName { get; set; }
        public int? Quantity { get; set; }
    }
}
