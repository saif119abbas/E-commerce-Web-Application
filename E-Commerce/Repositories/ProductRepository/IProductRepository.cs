using E_Commerce.Models;
using E_Commerce.Utilities;
using MongoDB.Driver;


namespace E_Commerce.Repositories
{
    public interface IProductRepository
    {
        Task<OperationResult<Product>> AddProductAsync(Product product, IClientSessionHandle session = null);
        Task<OperationResult<Product>> DeleteProductAsync(Guid vendorId, Guid productId, IClientSessionHandle session = null);
        Task<OperationResult<Product>> EditProductAsync(Guid vendorId,Guid productId, Product product, IClientSessionHandle session = null);
        Task<OperationResult<List<Product>>> EditProductsAsync(List<Product> products, IClientSessionHandle session = null);
        Task<OperationResult<List<Product>>> GetProductsAsync();
        Task<OperationResult<List<Product>>> GetProductsAsync(string categoryName);
        Task<OperationResult<List<Product>>> GetProductsAsync(Guid vendorId);
        Task<OperationResult<List<Product>>> GetProductsAsync(List<Guid> productIds);
        Task<OperationResult<Product>> GetProductAsync(Guid productId);
        Task<OperationResult<Product>> GetProductAsync(Guid vendorId, Guid productId);
    }
}
