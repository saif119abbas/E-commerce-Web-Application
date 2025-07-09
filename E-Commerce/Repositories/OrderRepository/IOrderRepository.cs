using E_Commerce.Models;
using E_Commerce.Utilities;
using MongoDB.Driver;

namespace E_Commerce.Repositories
{
    public interface IOrderRepository
    {
        Task<OperationResult<Order>> CreateOrderAsync(Order order,IClientSessionHandle session=null);
        Task<OperationResult<List<Order>>> GetOrdersAsync(Guid userId);
    }
}
