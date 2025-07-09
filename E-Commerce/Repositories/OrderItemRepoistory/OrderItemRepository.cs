using E_Commerce.Models;
using E_Commerce.Utilities;
using MongoDB.Driver;

namespace E_Commerce.Repositories
{
    public class OrderItemRepository : MongoRepository<OrderItem>, IOrderItemRepository
    {
        public OrderItemRepository(IMongoDatabase database, IUnitOfWork unitOfWork)
    : base(database, unitOfWork, "order_items")
        {
        }
        public async Task<OperationResult<List<OrderItem>>> AddBulkAsync(List<OrderItem> orderItems, IClientSessionHandle session = null)
        {
            try
            {
                if (session != null)
                {
                    await _collection.InsertManyAsync(session, orderItems);
                }
                else
                {
                    await _collection.InsertManyAsync(orderItems);
                }
                return OperationResult<List<OrderItem>>.SuccessResult(orderItems);
            }
            catch (Exception ex)
            {
                switch (ex)
                {
                    case MongoWriteException mwe when mwe.WriteError.Category == ServerErrorCategory.DuplicateKey:
                    case MongoCommandException mce when mce.Code == 11000:
                        return OperationResult<List<OrderItem>>.FailureResult(409, "Duplicate key violation");

                    case TimeoutException:
                        return OperationResult<List<OrderItem>>.FailureResult(504, "Database operation timed out");

                    case MongoException me:
                        return OperationResult<List<OrderItem>>.FailureResult(500, $"Database error: {me.Message}");

                    default:
                        return OperationResult<List<OrderItem>>.FailureResult(500, $"Unexpected error: {ex.Message}");
                }
            }
        }

        public async Task<OperationResult<List<OrderItem>>> GetItemsAsync(Guid orderId,Guid customerId)
        {
            try
            {
                var result = await _collection.Find(o => o.OrderId==orderId && o.CustomerId==customerId).ToListAsync();
                return OperationResult<List<OrderItem>>.SuccessResult(result);
            }
            catch (Exception ex)
            {
                switch (ex)
                {
                    case MongoCommandException mce when mce.Code == 11000:
                        return OperationResult<List<OrderItem>>.FailureResult(409, "Duplicate key violation");

                    case TimeoutException:
                        return OperationResult<List<OrderItem>>.FailureResult(504, "Database operation timed out");

                    case MongoException me:
                        return OperationResult<List<OrderItem>>.FailureResult(500, $"Database error: {me.Message}");

                    default:
                        return OperationResult<List<OrderItem>>.FailureResult(500, $"Unexpected error: {ex.Message}");
                }
            }
        }
    }
}
