using Microsoft.AspNetCore.Mvc;

namespace E_Commerce.Controllers
{
    public class ErrorController : Controller
    {
        [Route("Error")]
        public IActionResult Index(int code = 500, string message = "An unexpected error occurred.")
        {
            ViewBag.StatusCode = code;
            ViewBag.ErrorMessage = message;
            ViewBag.PreviousUrl = Request.Headers["Referer"].ToString();
            return View();
        }
    }
}
