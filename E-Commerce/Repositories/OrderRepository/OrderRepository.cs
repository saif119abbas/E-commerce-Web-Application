using E_Commerce.Models;
using E_Commerce.Utilities;
using MongoDB.Driver;

namespace E_Commerce.Repositories
{
    public class OrderRepository : MongoRepository<Order>, IOrderRepository
    {
        public OrderRepository(IMongoDatabase database, IUnitOfWork unitOfWork) :
          base(database, unitOfWork, "Orders")
        {
        }
        public async Task<OperationResult<Order>> CreateOrderAsync(Order order, IClientSessionHandle session = null)
        {
            try
            {
                if (session == null)
                {
                    await _collection.InsertOneAsync(order);
                }
                else
                {
                    await _collection.InsertOneAsync(session,order);
                }
                return OperationResult<Order>.SuccessResult(order);
            }
            catch (Exception ex)
            {
                switch (ex)
                {
                    case MongoWriteException mwe when mwe.WriteError.Category == ServerErrorCategory.DuplicateKey:
                    case MongoCommandException mce when mce.Code == 11000:
                        return OperationResult<Order>.FailureResult(409, "Duplicate key violation");

                    case TimeoutException:
                        return OperationResult<Order>.FailureResult(504, "Database operation timed out");

                    case MongoException me:
                        return OperationResult<Order>.FailureResult(500, $"Database error: {me.Message}");

                    default:
                        return OperationResult<Order>.FailureResult(500, $"Unexpected error: {ex.Message}");
                }
            }
        }

        public async Task<OperationResult<List<Order>>> GetOrdersAsync(Guid userId, IClientSessionHandle session = null)
        {
            try
            {
                var result=await _collection.Find(o=>o.CustomerId==userId).ToListAsync();
                return OperationResult<List<Order>>.SuccessResult(result);
            }
            catch (Exception ex)
            {
                switch (ex)
                {
                    case MongoCommandException mce when mce.Code == 11000:
                        return OperationResult<List<Order>>.FailureResult(409, "Duplicate key violation");

                    case TimeoutException:
                        return OperationResult<List<Order>>.FailureResult(504, "Database operation timed out");

                    case MongoException me:
                        return OperationResult<List<Order>>.FailureResult(500, $"Database error: {me.Message}");

                    default:
                        return OperationResult<List<Order>>.FailureResult(500, $"Unexpected error: {ex.Message}");
                }
            }
        }
    }
}
