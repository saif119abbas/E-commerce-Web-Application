using E_Commerce.Models;
using E_Commerce.Utilities;
using MongoDB.Driver;

namespace E_Commerce.Repositories
{
    public interface IProductReservationRepository
    {
        Task<OperationResult<List<ProductReservation>>> ReserveProductsAsync(List<ProductReservation> product, IClientSessionHandle session = null);
        Task<OperationResult<List<ProductReservation>>> GetProductsAsync(List<Guid> productIds);
        Task<OperationResult<List<ProductReservation>>> EditProductsAsync(List<ProductReservation> product, IClientSessionHandle session = null);
        Task<OperationResult<List<ProductReservation>>> GetProductsForCurrentSessionsAsync(Guid customerId);
        Task<OperationResult<List<ProductReservation>>> DeleteAsync(List<Guid> products, IClientSessionHandle session = null);
    }
}
