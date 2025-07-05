using System.ComponentModel.DataAnnotations;

namespace E_Commerce.Models
{
    public class ProductModel
    {
        [Required(ErrorMessage = "Please enter the name of the product")]
        public string? Name { get; set; }

        [Required(ErrorMessage = "Please enter the price")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
 
        public decimal? Price { get; set; }
        [Required(ErrorMessage = "Please enter the qunatity")]
        public int? Quantity { get; set; } = 0;

        [Required(ErrorMessage = "Please select the category")]
        public string? CategoryName { get; set; }
    }
}
