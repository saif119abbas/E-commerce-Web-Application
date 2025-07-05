using System.ComponentModel.DataAnnotations;

namespace E_Commerce.Models
{
    public class UserModel
    {
        [Required(ErrorMessage ="The username is required")]
        public string? Username { get; set; }
        [Required(ErrorMessage = "Please enter your password")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*()_+{}\[\]:;<>,.?~\\/-]).{8,}$",
         ErrorMessage = "Password must be at least 8 characters long and include uppercase, lowercase, number, and special character.")]
        public string? Password { get; set; }
        [Required(ErrorMessage = "The Email is required")]
        [EmailAddress(ErrorMessage = "need valid email")]
        public string? Email { get; set; }
        [Required(ErrorMessage = "The role is required")]
        public string? Role { get; set; } = Roles.Customer;
    }
}
