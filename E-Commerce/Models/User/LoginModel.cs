using System.ComponentModel.DataAnnotations;

namespace E_Commerce.Models
{
    public class LoginModel
    {
        [EmailAddress(ErrorMessage = "Invalid email")]
        [Required(ErrorMessage = "Please enter your email")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Please enter your password")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*()_+{}\[\]:;<>,.?~\\/-]).{8,}$",
            ErrorMessage = "Password must be at least 8 characters long and include uppercase, lowercase, number, and special character.")]
        public string? Password { get; set; }
    }
}
