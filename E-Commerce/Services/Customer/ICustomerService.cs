using E_Commerce.Models;
using E_Commerce.Utilities;
using Stripe.Checkout;

namespace E_Commerce.Services
{
    public interface ICustomerService
    {
        Task<OperationResult<List<ProductItem>>> GetAllProductsAllAsync();
        Task<OperationResult<CartViewModel>> GetCartAsync(string userId);
        Task<OperationResult<CartViewModel>> UpdateCartAsync(string userId,string productId, int quantity = 1);
        Task<OperationResult<CheckoutSessionResponse>> CheckoutAsync(string userId);
        Task<OperationResult<bool>> HandlePaymentSuccessAsync(Session session);
        Task<OperationResult<bool>> HandlePaymentFailureAsync(Session session);
        Task<OperationResult<List<ProductItem>>> SearchAsync(string searchQuery);
        Task<OperationResult<List<Order>>> GetOrders(string customerId);
    }
}
