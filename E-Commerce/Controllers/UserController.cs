using E_Commerce.Models;
using E_Commerce.Services;
using Microsoft.AspNetCore.Mvc;

namespace E_Commerce.Controllers
{
    public class UserController : Controller
    {
        private readonly IUserService _userService;
        public UserController(IUserService userService)
        {
           _userService = userService;
        }
        public IActionResult Login()
        {
            return View();
        }
        public async Task<IActionResult> Signup()
        {
            var roles = await _userService.GetAllRolesAsSelectListAsync();

            var viewModel = new SignupViewModel
            {
                Roles = roles
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginModel model)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction("Index", "Error", new { code = 400, message = "Invalid login parameters." });
            }

            var result = await _userService.LoginAsync(model);

            if (result == null)
            {
                return RedirectToAction("Index", "Error", new { code = 500, message = "Unexpected null response from login service." });
            }

            if (!result.Success)
            {
                string errorMessage = string.Join("; ", result.Errors);
                return RedirectToAction("Index", "Error", new { code = result.StatusCode, message = errorMessage });
            }

            if (result.Data == null || result.Data.token == null || result.Data.Role == null)
            {
                return RedirectToAction("Index", "Error", new { code = 500, message = "Login data was incomplete or invalid." });
            }

            Response.Cookies.Append("jwtToken", result.Data.token, new CookieOptions
            {
                HttpOnly = false,
                Secure = true
            });

            if (result.Data.Role == Roles.Vendor)
            {
                return RedirectToAction("Index", "Vendor");
            }

            return RedirectToAction("Index", "Customer");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Signup([Bind("Username,Email,Password,Role")]UserModel user)
        {
            if (ModelState.IsValid)
            {
                var result=await _userService.SignupAsync(user);
                if(result==null)
                    return RedirectToAction("Index", "Error", new { code = 500, message = "Something went wrong" });
                if(!result.Success)
                {
                    return RedirectToAction("Index", "Error", new { code = result.StatusCode, message = result.Errors });
                }

                if(result.Data==null)
                    return RedirectToAction("Index", "Error", new { code = 500, message = "Something went wrong" });

                return RedirectToAction("Login", "User");
            }
                return BadRequest("Invalid Parameter");
        }
    }
}
