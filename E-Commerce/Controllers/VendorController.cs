using Microsoft.AspNetCore.Mvc;
using E_Commerce.Models;
using Microsoft.AspNetCore.Authorization;
using E_Commerce.Services;
using Microsoft.AspNetCore.Mvc.Rendering;


namespace E_Commerce.Controllers
{
    [Authorize(Roles = "vendor")]
    public class VendorController : BaseController
    {
 
        private readonly IVendorService _vendorService;
     
        public VendorController(IVendorService vendorService,IJwtProvider jwtProvider):base(jwtProvider) 
        {
            _vendorService = vendorService;
         
        }
        public async Task<ActionResult> Index()
        {
            if (UserId == null)
            {
                return RedirectToAction("Index", "Error", new
                {
                    code = 401,
                    message = "UnAuthorized User"
                });
            }
            var result = await _vendorService.GetVendorProductsAsync(UserId);

            if (!result.Success)
            {
                return RedirectToAction("Index", "Error", new
                {
                    code = result.StatusCode,
                    message = result.Errors
                });
            }

            return View(result.Data); 
        }
        public async Task<IActionResult> EditProduct(string productId)
        {

            if (UserId == null)
            {
                return RedirectToAction("Index", "Error", new
                {
                    code = 401,
                    message = "UnAuthorized User"
                });
            }
            ViewBag.ProductId = productId;
            var getCategoriesResult = await _vendorService.GetAllCategoriesAsync();
            if (!getCategoriesResult.Success)
                return RedirectToAction("Index", "Error", new
                {
                    code = getCategoriesResult.StatusCode,
                    message = getCategoriesResult.Errors
                });

            var getProductResult = await  _vendorService.GetProductAsync(UserId, productId);
            if(!getProductResult.Success)
                return RedirectToAction("Index", "Error", new { code = getProductResult.StatusCode, 
                    message = getProductResult.Errors});
            var data=getProductResult.Data;
            var productModel = new ProductModel
            {
                Name = data!.Name,
                Price = data.Price,
                CategoryName = data.CategoryName,
                Quantity = data.Quantity,
            
            };

            var viewModel = new ProductViewModel
            {
                Product = productModel,
                Categories = getCategoriesResult.Data!.Select(c => new SelectListItem
                {
                    Value = c.Name,
                    Text= c.Name,
                }
               ).ToList()
            };
            return View(viewModel);
        }

        public async Task<IActionResult> AddProduct(string vendorId)
        {
            ViewBag.VendorId = vendorId;
            var getCategoriesResult = await _vendorService.GetAllCategoriesAsync();
            if (!getCategoriesResult.Success)
                return RedirectToAction("Index", "Error", new
                {
                    code = getCategoriesResult.StatusCode,
                    message = getCategoriesResult.Errors
                });

            var viewModel = new ProductViewModel
            {
                Categories = getCategoriesResult.Data!.Select(c => new SelectListItem
                {
                    Value = c.Name,
                    Text = c.Name,
                }
               ).ToList()
            };

     
            return View(viewModel);
        }
        public async Task<IActionResult> DeleteProduct( string productId)
        {
         
            if (UserId == null)
            {
                return RedirectToAction("Index", "Error", new
                {
                    code = 401,
                    message = "UnAuthorize user"
                });
            }
         
            ViewBag.ProductId = productId;
            var getProductResult=await _vendorService.GetProductAsync(UserId, productId);
            if (!getProductResult.Success)
                return RedirectToAction("Index", "Error", new
                {
                    code = getProductResult.StatusCode,
                    message = getProductResult.Errors
                });
            var model = new ProductModel
            {
                Name = getProductResult.Data!.Name,
                Price = getProductResult.Data!.Price,
                Quantity = getProductResult.Data!.Quantity,
                CategoryName = getProductResult.Data!.CategoryName,
            };
            return View(model);

        }
        [HttpPost, ActionName("DeleteProduct")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string productId)
        {
           
          
            if (UserId == null)
            {
                return RedirectToAction("Index", "Error", new
                {
                    code = 401,
                    message = "UnAuthorize user"
                });
            }
            var result = await _vendorService.DeleteProductAsync(UserId, productId);
            if (!result.Success)
                return RedirectToAction("Index", "Error", new
                {
                    code = result.StatusCode,
                    message = result.Errors
                });
            return RedirectToAction("Index", "Vendor");

        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditProduct(string productId,
        [Bind("Name,Price,Quantity,CategoryName")] ProductModel product)
        {
          
            if (UserId == null)
            {
                return RedirectToAction("Index", "Error", new
                {
                    code = 401,
                    message = "UnAuthorize user"
                });
            }
            if (!ModelState.IsValid || product == null)
            {
                return RedirectToAction("Index", "Error", new { code = 400, message = "Invalid parameters" });
            }
            var result = await _vendorService.UpdateProductAsync(UserId, productId, product);
            if (result == null)
            {
                return RedirectToAction("Index", "Error", new { code = 500, message = "Something went wrong" });
            }
            if (!result.Success)
            {
                return RedirectToAction("Index", "Error", new { code = result.StatusCode, message = result.Errors });
            }
            if (result.Data == null || result.Data.Name == null || result.Data.CategoryName == null || result.Data.Price == null || result.Data.Quantity == null)
            {
                return RedirectToAction("Index", "Error", new { code = 500, message = "Something went wrong" });
            }
            return RedirectToAction("Index", "Vendor");


        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddProduct(
            [Bind("Name,Price,Quantity,CategoryName")] ProductModel product)
        {
            if (!ModelState.IsValid || product ==null)
            {
                return RedirectToAction("Index", "Error", new { code = 400, message = "Invalid parameters" });
            }
           
            if (UserId == null)
            {
                return RedirectToAction("Index", "Error", new
                {
                    code = 401,
                    message = "UnAuthorize user"
                });
            }
            var result = await _vendorService.AddProductAsync(UserId, product);
            if(result==null)
            {
                return RedirectToAction("Index", "Error", new { code = 500, message = "Something went wrong" });
            }
            if (!result.Success)
            {
                return RedirectToAction("Index", "Error", new { code = result.StatusCode, message =result.Errors });
            }
            if (result.Data == null || result.Data.Name==null || result.Data.CategoryName==null || result.Data.Price==null || result.Data.Quantity==null)
            {
                return RedirectToAction("Index", "Error", new { code = 500, message = "Something went wrong" });
            }
            return RedirectToAction("Index", "Vendor");

        }
     
 
    }
}
