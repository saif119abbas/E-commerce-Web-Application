using E_Commerce.Models;
using E_Commerce.Utilities;
using MongoDB.Driver;

namespace E_Commerce.Repositories
{
    public interface IOrderItemRepository
    {
        Task<OperationResult<List<OrderItem>>> AddBulkAsync(List<OrderItem> orderItems, IClientSessionHandle session = null);
        Task<OperationResult<List<OrderItem>>> GetItemsAsync(Guid orderId, Guid customerId);

    }
}
