using E_Commerce.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace E_Commerce.Controllers
{
    [Authorize]
    public class BaseController : Controller
    {
        private readonly IJwtProvider _jwtProvider;
        protected string? UserId;

        public BaseController(IJwtProvider jwtProvider)
        {
            _jwtProvider = jwtProvider;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);

            var token = Request.Cookies["jwtToken"];
            if (!string.IsNullOrEmpty(token))
            {
                UserId = _jwtProvider.GetUserIdFromToken(token);
            }
        }
    }
}
