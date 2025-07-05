using E_Commerce.Models;
using E_Commerce.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nest;
using Stripe.Checkout;
using Stripe.Climate;

namespace E_Commerce.Controllers
{
    [Authorize(Roles = "customer")]
    public class CustomerController : BaseController
    {
        private readonly ICustomerService _customerService;
        public CustomerController(ICustomerService customerService, IJwtProvider jwtProvider):base(jwtProvider) 
        {
            _customerService = customerService;
        }
        public async Task<ActionResult> Index(string searchQuery)
        {
          
           
            var result= searchQuery==null?
                await _customerService.GetAllProductsAllAsync() :
                 await _customerService.SearchAsync(searchQuery)
                ;
            if(result == null || result.Data==null || !result.Success)
            {
                var error = result?.Errors != null
                       ? string.Join(", ", result.Errors)
                       : "Something went wrong";
                return RedirectToAction("Index", "Error", new
                {
                    code = result?.StatusCode ?? 500,
                    message =error
                });
            }
         
            return View(result.Data);
        }
        public async Task<ActionResult> Cart()
        {
            if (UserId == null)
            {
                return RedirectToAction("Index", "Error", new
                {
                    code = 401,
                    message = "UnAuthorize user"
                });
            }
         
            var result = await _customerService.GetCartAsync(UserId);
            if (result == null || result.Data == null || !result.Success)
            {
                var error = result?.Errors != null
                       ? string.Join(", ", result.Errors)
                       : "Something went wrong";
                return RedirectToAction("Index", "Error", new
                {
                    code = result?.StatusCode ?? 500,
                    message = error
                });
            }

            return View(result.Data);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddToCart(string productId)
        {
            if (UserId == null)
            {
                return RedirectToAction("Index", "Error", new
                {
                    code = 401,
                    message = "UnAuthorize user"
                });
            }

            var result = await _customerService.UpdateCartAsync(UserId,productId);
            if (result == null || result.Data == null || !result.Success)
            {
                var error = result?.Errors != null
                       ? string.Join(", ", result.Errors)
                       : "Something went wrong";
                return RedirectToAction("Index", "Error", new
                {
                    code = result?.StatusCode ?? 500,
                    message = error
                });
            }
            return RedirectToAction("Index");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RemoveFromCart(string productId)
        {
            if (UserId == null)
            {
                return RedirectToAction("Index", "Error", new
                {
                    code = 401,
                    message = "UnAuthorize user"
                });
            }

            var result = await _customerService.UpdateCartAsync(UserId, productId,0);
            if (result == null || result.Data == null || !result.Success)
            {
                var error = result?.Errors != null
                       ? string.Join(", ", result.Errors)
                       : "Something went wrong";
                return RedirectToAction("Index", "Error", new
                {
                    code = result?.StatusCode ?? 500,
                    message = error
                });
            }
            return RedirectToAction("Cart");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Checkout()
        {
            if (UserId == null)
            {
                return RedirectToAction("Index", "Error", new
                {
                    code = 401,
                    message = "UnAuthorize user"
                });
            }
            var result = await _customerService.CheckoutAsync(UserId);
            if (result == null || result.Data == null || !result.Success)
            {
                var error = result?.Errors != null
                       ? string.Join(", ", result.Errors)
                       : "Something went wrong";
                return RedirectToAction("Index", "Error", new
                {
                    code = result?.StatusCode ?? 500,
                    message = error
                });
            }
            Response.Headers.Add("Location", result.Data.Url);
            return new StatusCodeResult(303);

        }
        public  async Task<ActionResult> Orders()
        {
            if (UserId == null)
            {
                return RedirectToAction("Index", "Error", new
                {
                    code = 401,
                    message = "UnAuthorize user"
                });
            }
            var result = await _customerService.GetOrders(UserId);
            if (result == null || result.Data == null || !result.Success)
            {
                var error = result?.Errors != null
                       ? string.Join(", ", result.Errors)
                       : "Something went wrong";
                return RedirectToAction("Index", "Error", new
                {
                    code = result?.StatusCode ?? 500,
                    message = error
                });
            }
            return View();

        }
 

    }

}
