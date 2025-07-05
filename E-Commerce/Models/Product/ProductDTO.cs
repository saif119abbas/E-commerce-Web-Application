namespace E_Commerce.Models
{
    public class ProductDTO
    {
        public Guid Id {get; set;}
        public string ?Name { get; set;}
        public decimal? Price { get; set;}
        public string? CategoryName { get; set;}
        public int? Quantity { get; set;}
    }
}
