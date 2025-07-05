
using System.ComponentModel.DataAnnotations;
namespace E_Commerce.Models
{
    public class VendorModel
    {
        [Required(ErrorMessage = "The Id for the user is required")]
        public Guid UserId { get; set; }
        [Required(ErrorMessage = "The username is required")]
        public string? UserName { get; set; }
        [Required(ErrorMessage = "The Email is required")]
        [EmailAddress(ErrorMessage = "please enter a valid email")]
        public string? Email { get; set; }
    }
}
