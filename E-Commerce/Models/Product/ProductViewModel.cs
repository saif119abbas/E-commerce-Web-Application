using Microsoft.AspNetCore.Mvc.Rendering;

namespace E_Commerce.Models
{
    public class ProductViewModel
    {
        public ProductModel Product { get; set; } = new();
        public List<SelectListItem> Categories { get; set; } = new();
    }
}
