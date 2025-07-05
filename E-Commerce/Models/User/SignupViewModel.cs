using Microsoft.AspNetCore.Mvc.Rendering;

namespace E_Commerce.Models
{
    public class SignupViewModel
    {
        public UserModel User { get; set; } = new();
        public List<SelectListItem> Roles { get; set; } = new();
    }

}
