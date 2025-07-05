using E_Commerce.Models;
using E_Commerce.Utilities;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace E_Commerce.Services
{
    public interface IVendorService
    {
        Task<OperationResult<ProductDTO>> AddProductAsync(string vendorId, ProductModel product);
        Task<OperationResult<ProductDTO>> UpdateProductAsync(string vendorId, string productId,ProductModel product);
        Task<OperationResult<ProductDTO>> DeleteProductAsync(string vendorId, string productId);
        Task<OperationResult<List<ProductDTO>>> GetVendorProductsAsync(string vendorId, bool forceRefresh = false);
        Task<OperationResult<ProductDTO>> GetProductAsync(string vendorId,string productId);
        Task<OperationResult<List<CategoryDTO>>> GetAllCategoriesAsync();
    }
}
