using E_Commerce.Models;
using E_Commerce.Utilities;
using MongoDB.Driver;

namespace E_Commerce.Repositories
{
    public class ProductReservationRepository : MongoRepository<ProductReservation>, IProductReservationRepository
    {
        public ProductReservationRepository(IMongoDatabase database, IUnitOfWork unitOfWork)
        : base(database, unitOfWork, "product_reservations")
        {
        }

        public async Task<OperationResult<List<ProductReservation>>> DeleteAsync(List<Guid> products, IClientSessionHandle session = null)
        {
            try
            {
              
                var filter = Builders<ProductReservation>.Filter.In(r => r.Id, products);

                DeleteResult result = session == null
                    ? await _collection.DeleteManyAsync(filter)
                    : await _collection.DeleteManyAsync(session, filter);

                if (result.DeletedCount != products.Count)
                {
                    var missingCount = products.Count - result.DeletedCount;
                    return OperationResult<List<ProductReservation>>.FailureResult(
                        404,
                        $"Failed to delete {missingCount} reservations");
                }

                return OperationResult<List<ProductReservation>>.SuccessResult(null);
            }
            catch (MongoException ex)
            {
                
                return OperationResult<List<ProductReservation>>.FailureResult(
                    500,
                    "Database error while deleting reservations");
            }
        }

        public async Task<OperationResult<List<ProductReservation>>> EditProductsAsync(List<ProductReservation> products,
            IClientSessionHandle session = null)
        {
            try
            {
                var updatedProducts = new List<ProductReservation>();
                var bulkUpdates = new List<WriteModel<ProductReservation>>();

                // Prepare bulk operations
                foreach (var product in products)
                {
                    var filter = Builders<ProductReservation>.Filter.Eq(p => p.Id, product.Id);
                    var update = Builders<ProductReservation>.Update
                        .Set(p => p.IsConfirmed, product.IsConfirmed)
                        .Set(p => p.ReservedQuantity, product.ReservedQuantity)
                        .Set(p => p.ReservedAt, product.ReservedAt)
                        .Set(p => p.ExpiresAt, product.ExpiresAt)
                        .Set(p => p.ProductId, product.ProductId)
                        .Set(p => p.UserId, product.UserId);

                    bulkUpdates.Add(new UpdateOneModel<ProductReservation>(filter, update));
                }

                // Execute bulk operation
                BulkWriteResult<ProductReservation> result;
                if (session == null)
                {
                    result = await _collection.BulkWriteAsync(bulkUpdates);
                }
                else
                {
                    result = await _collection.BulkWriteAsync(session, bulkUpdates);
                }

                // Verify all were processed
                if (result.ModifiedCount != products.Count)
                {
                    return OperationResult<List<ProductReservation>>.FailureResult(
                        404,
                        $"Some products were not updated. Expected {products.Count}, updated {result.ModifiedCount}");
                }

                // Fetch updated documents
                var updatedIds = products.Select(p => p.Id).ToList();
                var filterByIds = Builders<ProductReservation>.Filter.In(p => p.Id, updatedIds);

                updatedProducts = session == null
                    ? await _collection.Find(filterByIds).ToListAsync()
                    : await _collection.Find(session, filterByIds).ToListAsync();

                return OperationResult<List<ProductReservation>>.SuccessResult(updatedProducts);
            }
            catch (Exception ex)
            {
                switch (ex)
                {
                    case MongoCommandException mce when mce.Code == 11000:
                        return OperationResult<List<ProductReservation>>.FailureResult(409, "Duplicate key violation");

                    case TimeoutException:
                        return OperationResult<List<ProductReservation>>.FailureResult(504, "Database operation timed out");

                    case MongoException me:
                        return OperationResult<List<ProductReservation>>.FailureResult(500, $"Database error: {me.Message}");

                    default:
                        return OperationResult<List<ProductReservation>>.FailureResult(500, $"Unexpected error: {ex.Message}");
                }
            }
        }

        public async Task<OperationResult<List<ProductReservation>>> GetProductsAsync(List<Guid> productIds)
        {
            try
            {

                var filter = Builders<ProductReservation>.Filter.In(p => p.Id, productIds);
                var products = await _collection.Find(filter).ToListAsync();
                return OperationResult<List<ProductReservation>>.SuccessResult(products);
            }
            catch (Exception ex)
            {
                switch (ex)
                {
                    case MongoCommandException mce when mce.Code == 11000:
                        return OperationResult<List<ProductReservation>>.FailureResult(409, "Duplicate key violation");

                    case TimeoutException:
                        return OperationResult<List<ProductReservation>>.FailureResult(504, "Database operation timed out");

                    case MongoException me:
                        return OperationResult<List<ProductReservation>>.FailureResult(500, $"Database error: {me.Message}");

                    default:
                        return OperationResult<List<ProductReservation>>.FailureResult(500, $"Unexpected error: {ex.Message}");
                }
            }
        }

        public async Task<OperationResult<List<ProductReservation>>> GetProductsForCurrentSessionsAsync(Guid customerId)
        {
            try
            {
                var products=await _collection.Find(p=>p.UserId == customerId && !p.IsConfirmed).ToListAsync();
                return OperationResult<List<ProductReservation>>.SuccessResult(products);
            }
             catch (Exception ex)
            {
                switch (ex)
                {
                    case MongoCommandException mce when mce.Code == 11000:
                        return OperationResult<List<ProductReservation>>.FailureResult(409, "Duplicate key violation");

                    case TimeoutException:
                        return OperationResult<List<ProductReservation>>.FailureResult(504, "Database operation timed out");

                    case MongoException me:
                        return OperationResult<List<ProductReservation>>.FailureResult(500, $"Database error: {me.Message}");

                    default:
                        return OperationResult<List<ProductReservation>>.FailureResult(500, $"Unexpected error: {ex.Message}");
                }
            }
            throw new NotImplementedException();
        }

        public async Task<OperationResult<List<ProductReservation>>> ReserveProductsAsync(List<ProductReservation> product, IClientSessionHandle session = null)
        {
            try
            {
                if (session != null)
                {
                    await _collection.InsertManyAsync(session, product);
                }
                else
                {
                    await _collection.InsertManyAsync(product);
                }
               return  OperationResult<List<ProductReservation>>.SuccessResult(product);
            }
            catch (Exception ex)
            {
                switch (ex)
                {
                    case MongoWriteException mwe when mwe.WriteError.Category == ServerErrorCategory.DuplicateKey:
                    case MongoCommandException mce when mce.Code == 11000:
                        return OperationResult<List<ProductReservation>>.FailureResult(409, "Duplicate key violation");

                    case TimeoutException:
                        return OperationResult<List<ProductReservation>>.FailureResult(504, "Database operation timed out");

                    case MongoException me:
                        return OperationResult<List<ProductReservation>>.FailureResult(500, $"Database error: {me.Message}");

                    default:
                        return OperationResult<List<ProductReservation>>.FailureResult(500, $"Unexpected error: {ex.Message}");
                }
            }
        }
    }
}
