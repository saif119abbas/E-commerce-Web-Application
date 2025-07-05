using E_Commerce.Models;
using E_Commerce.Utilities;
using Microsoft.CodeAnalysis;
using MongoDB.Driver;

namespace E_Commerce.Repositories
{
    public class CartRepositroy : MongoRepository<Cart>, ICartRepository
    {
        public CartRepositroy(IMongoDatabase database, IUnitOfWork unitOfWork ) : 
            base(database, unitOfWork, "Carts")
        {
        }

        public async Task<OperationResult<Cart>> AddCartAsync(Cart cart, IClientSessionHandle session = null)
        {
            try
            {
                if (session != null)
                {
                    await _collection.InsertOneAsync(session, cart);
                }
                else
                {
                    await _collection.InsertOneAsync(cart);
                }
               
                return OperationResult<Cart>.SuccessResult(cart);
               
            }
            catch (Exception ex)
            {
                switch (ex)
                {
                    case MongoWriteException mwe when mwe.WriteError.Category == ServerErrorCategory.DuplicateKey:
                    case MongoCommandException mce when mce.Code == 11000:
                        return OperationResult<Cart>.FailureResult(409, "Duplicate key violation");

                    case TimeoutException:
                        return OperationResult<Cart>.FailureResult(504, "Database operation timed out");

                    case MongoException me:
                        return OperationResult<Cart>.FailureResult(500, $"Database error: {me.Message}");

                    default:
                        return OperationResult<Cart>.FailureResult(500, $"Unexpected error: {ex.Message}");
                }
            }
        }
        public async Task<OperationResult<Cart>> UpdateCartAsync(Cart cart, IClientSessionHandle session = null)
        {
            try
            {
                var options = new ReplaceOptions
                {
                    IsUpsert = true
                };
                var filter = Builders<Cart>.Filter.Eq(c => c.UserId, cart.UserId);

                var updatedCart = session == null ?
                    await _collection.ReplaceOneAsync(filter, cart, options) :
                    await _collection.ReplaceOneAsync(session,filter, cart, options);

                if (!updatedCart.IsAcknowledged)
                {
                    return OperationResult<Cart>.FailureResult(500, "Database operation not acknowledged");
                }
                if (updatedCart == null|| updatedCart.UpsertedId != null || updatedCart.MatchedCount==0)
                    return OperationResult<Cart>.FailureResult(404, "Cart not found");

                return OperationResult<Cart>.SuccessResult(cart);


            }
            catch (Exception ex)
            {

                switch (ex)
                {
                    case MongoWriteException mwe when mwe.WriteError.Category == ServerErrorCategory.DuplicateKey:
                    case MongoCommandException mce when mce.Code == 11000:
                        return OperationResult<Cart>.FailureResult(409, "Duplicate key violation");

                    case TimeoutException:
                        return OperationResult<Cart>.FailureResult(504, "Database operation timed out");

                    case MongoException me:
                        return OperationResult<Cart>.FailureResult(500, $"Database error: {me.Message}");

                    default:
                        return OperationResult<Cart>.FailureResult(500, $"Unexpected error: {ex.Message}");
                }
            }
        }

        public async Task<OperationResult<Cart>> GetCartAsync(Guid userId)
        {
            try
            {
                var cart = await _collection.Find(c => c.UserId == userId).FirstOrDefaultAsync();

                if (cart == null)
                {
                    return OperationResult<Cart>.FailureResult(400, "The cart is not avilable right now");
                }
                return OperationResult<Cart>.SuccessResult(cart);
            }
            catch (Exception ex)
            {
                switch (ex)
                {
                    case MongoCommandException mce when mce.Code == 11000:
                        return OperationResult<Cart>.FailureResult(409, "Duplicate key violation");

                    case TimeoutException:
                        return OperationResult<Cart>.FailureResult(504, "Database operation timed out");

                    case MongoException me:
                        return OperationResult<Cart>.FailureResult(500, $"Database error: {me.Message}");

                    default:
                        return OperationResult<Cart>.FailureResult(500, $"Unexpected error: {ex.Message}");
                }
            }
        }

    }
}
