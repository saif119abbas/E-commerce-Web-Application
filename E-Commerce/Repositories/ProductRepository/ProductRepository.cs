using E_Commerce.Models;
using E_Commerce.Utilities;
using MongoDB.Driver;
namespace E_Commerce.Repositories
{
    public class ProductRepository : MongoRepository<Product>, IProductRepository
    {
      
        public ProductRepository(IMongoDatabase database, IUnitOfWork unitOfWork)
         : base(database, unitOfWork, "Products")
        {
        }
        public async Task<OperationResult<Product>> AddProductAsync(Product product, IClientSessionHandle session=null)
        {
            try
            {
                if (session == null)
                {
                
                    await _collection.InsertOneAsync(product);
                }
                else
                {
                    await _collection.InsertOneAsync(session ,product);
                }

                return OperationResult<Product>.SuccessResult(product);
            }
            catch (MongoWriteException ex) when (ex.WriteError.Category == ServerErrorCategory.DuplicateKey)
            {
                return OperationResult<Product>.FailureResult(409, "Product with this ID already exists");
             
            }
            catch (MongoCommandException ex) when (ex.Code == 11000)
            {
               
                return OperationResult<Product>.FailureResult(409, "Duplicate key violation");
            }
            catch (TimeoutException ex)
            {
            
                return OperationResult<Product>.FailureResult(504, "Database operation timed out");
            }
            catch (MongoException ex)
            {
      
                return OperationResult<Product>.FailureResult(500, $"Database error: {ex.Message}");
            }
            catch (Exception ex)
            {
                return OperationResult<Product>.FailureResult(500, $"Unexpected error: {ex.Message}");
            }
        }
        public async Task<OperationResult<Product>> DeleteProductAsync(Guid vendorId,Guid productId, IClientSessionHandle session)
        {
            try
            {

                var deleteResult = session == null ? await _collection.DeleteOneAsync(p => p.Id == productId && p.VendorId == vendorId) :
                                                await _collection.DeleteOneAsync(session, p => p.Id == productId && p.VendorId == vendorId);

                if (deleteResult.DeletedCount == 0)
                {
                    return OperationResult<Product>.FailureResult(404, "Product not found or already deleted");
                }

                return OperationResult<Product>.SuccessResult(null); 
            }
            catch (Exception ex)
            {
                switch (ex)
                {
                    case MongoCommandException mce when mce.Code == 11000:
                        return OperationResult<Product>.FailureResult(409, "Duplicate key violation");

                    case TimeoutException:
                        return OperationResult<Product>.FailureResult(504, "Database operation timed out");

                    case MongoException me:
                        return OperationResult<Product>.FailureResult(500, $"Database error: {me.Message}");

                    default:
                        return OperationResult<Product>.FailureResult(500, $"Unexpected error: {ex.Message}");
                }
            }
        }

        public async Task<OperationResult<Product>> EditProductAsync(Guid vendorId, Guid productId, Product product, IClientSessionHandle session=null)
        {
            try
            {
                var update = Builders<Product>.Update
                    .Set(p => p.Name, product.Name)
                    .Set(p => p.Price, product.Price)
                    .Set(p => p.CategoryId, product.CategoryId)
                    .Set(p => p.Quantity, product.Quantity)
                    .Set(p => p.CategoryName, product.CategoryName);

                var options = new FindOneAndUpdateOptions<Product>
                {
                    ReturnDocument = ReturnDocument.After 
                };

                var updatedProduct = session == null ?
                    await _collection.FindOneAndUpdateAsync<Product>(
                    p => p.VendorId == vendorId && p.Id == productId,
                    update,
                    options) :
                    await _collection.FindOneAndUpdateAsync<Product>(session,
                    p => p.VendorId == vendorId && p.Id == productId,
                    update,
                    options);

                if (updatedProduct == null)
                    return OperationResult<Product>.FailureResult(404, "Product not found");

                return OperationResult<Product>.SuccessResult(updatedProduct);
            }
            catch (Exception ex)
            {
                switch (ex)
                {
                    case MongoCommandException mce when mce.Code == 11000:
                        return OperationResult<Product>.FailureResult(409, "Duplicate key violation");

                    case TimeoutException:
                        return OperationResult<Product>.FailureResult(504, "Database operation timed out");

                    case MongoException me:
                        return OperationResult<Product>.FailureResult(500, $"Database error: {me.Message}");

                    default:
                        return OperationResult<Product>.FailureResult(500, $"Unexpected error: {ex.Message}");
                }
            }
        }
    
    

        public async Task<OperationResult<Product>> GetProductAsync(Guid productId)
        {
            try
            {
             
                Product result = await _collection.Find(p=>p.Id == productId).FirstOrDefaultAsync();
                if(result == null )
                {
                    return OperationResult<Product>.FailureResult(404, "The product doesn't exsit");
                }

                return OperationResult<Product>.SuccessResult(result);
            }
            catch (Exception ex)
            {
                switch (ex)
                {
                    case MongoCommandException mce when mce.Code == 11000:
                        return OperationResult<Product>.FailureResult(409, "Duplicate key violation");

                    case TimeoutException:
                        return OperationResult<Product>.FailureResult(504, "Database operation timed out");

                    case MongoException me:
                        return OperationResult<Product>.FailureResult(500, $"Database error: {me.Message}");

                    default:
                        return OperationResult<Product>.FailureResult(500, $"Unexpected error: {ex.Message}");
                }
            }
        }
     

    

        public async Task<OperationResult<List<Product>>> GetProductsAsync()
        {
            try
            {
                var result = await _collection.Find(_ => true).ToListAsync();
                return OperationResult<List<Product>>.SuccessResult(result);
            }
            catch (Exception ex)
            {
                switch (ex)
                {
                    case MongoCommandException mce when mce.Code == 11000:
                        return OperationResult<List<Product>>.FailureResult(409, "Duplicate key violation");

                    case TimeoutException:
                        return OperationResult<List<Product>>.FailureResult(504, "Database operation timed out");

                    case MongoException me:
                        return OperationResult<List<Product>>.FailureResult(500, $"Database error: {me.Message}");

                    default:
                        return OperationResult<List<Product>>.FailureResult(500, $"Unexpected error: {ex.Message}");
                }
            }
        }

        public async Task<OperationResult<List<Product>>> GetProductsAsync(Guid vendorId)
        {
            try
            {

                List<Product> result = await _collection.Find(p => p.VendorId == vendorId).ToListAsync();
                return OperationResult<List<Product>>.SuccessResult(result);
            }
            catch (Exception ex)
            {
                switch (ex)
                {
                    case MongoCommandException mce when mce.Code == 11000:
                        return OperationResult<List<Product>>.FailureResult(409, "Duplicate key violation");

                    case TimeoutException:
                        return OperationResult<List<Product>>.FailureResult(504, "Database operation timed out");

                    case MongoException me:
                        return OperationResult<List<Product>>.FailureResult(500, $"Database error: {me.Message}");

                    default:
                        return OperationResult<List<Product>>.FailureResult(500, $"Unexpected error: {ex.Message}");
                }
            }
        }

        public async Task<OperationResult<Product>> GetProductAsync(Guid vendorId, Guid productId)
        {
            try
            {
                var product = await _collection.Find(p => p.Id == productId && p.VendorId == vendorId).FirstOrDefaultAsync();
                if (product == null)
                {
                    return OperationResult<Product>.FailureResult(404, "This product didn't found");
                }
                return OperationResult<Product>.SuccessResult(product);

            }
            catch (Exception ex)
            {
                switch (ex)
                {
                    case MongoCommandException mce when mce.Code == 11000:
                        return OperationResult<Product>.FailureResult(409, "Duplicate key violation");

                    case TimeoutException:
                        return OperationResult<Product>.FailureResult(504, "Database operation timed out");

                    case MongoException me:
                        return OperationResult<Product>.FailureResult(500, $"Database error: {me.Message}");

                    default:
                        return OperationResult<Product>.FailureResult(500, $"Unexpected error: {ex.Message}");
                }
            }
        }
        public async Task<OperationResult<List<Product>>> GetProductsAsync(string categoryName)
        {
            try
            {
                var products = await _collection.Find(p => p.CategoryName==categoryName).ToListAsync();
                if (products == null)
                {
                    return OperationResult<List<Product>>.FailureResult(404, "This product didn't found");
                }
                return OperationResult<List<Product>>.SuccessResult(products);

            }
            catch (Exception ex)
            {
                switch (ex)
                {
                    case MongoCommandException mce when mce.Code == 11000:
                        return OperationResult<List<Product>>.FailureResult(409, "Duplicate key violation");

                    case TimeoutException:
                        return OperationResult<List<Product>>.FailureResult(504, "Database operation timed out");

                    case MongoException me:
                        return OperationResult<List<Product>>.FailureResult(500, $"Database error: {me.Message}");

                    default:
                        return OperationResult<List<Product>>.FailureResult(500, $"Unexpected error: {ex.Message}");
                }
            }
        }
        private async Task<Product> findProduct(Guid productId) =>
         await _collection.Find(p => p.Id == productId).FirstOrDefaultAsync();

        public async Task<OperationResult<List<Product>>> GetProductsAsync(List<Guid> productIds)
        {
            try
            {
             
                var filter = Builders<Product>.Filter.In(p => p.Id, productIds);
                var products = await _collection.Find(filter).ToListAsync();
                if (products.Count != productIds.Count)
                {
                    var foundIds = products.Select(p => p.Id).ToHashSet();
                    var missingIds = productIds.Where(id => !foundIds.Contains(id)).ToList();

                    return OperationResult<List<Product>>.FailureResult(404, $"Products not found: {string.Join(", ", missingIds)}");
                }

                return OperationResult<List<Product>>.SuccessResult(products);
            }
            catch (Exception ex)
            {
                switch (ex)
                {
                    case MongoCommandException mce when mce.Code == 11000:
                        return OperationResult<List<Product>>.FailureResult(409, "Duplicate key violation");

                    case TimeoutException:
                        return OperationResult<List<Product>>.FailureResult(504, "Database operation timed out");

                    case MongoException me:
                        return OperationResult<List<Product>>.FailureResult(500, $"Database error: {me.Message}");

                    default:
                        return OperationResult<List<Product>>.FailureResult(500, $"Unexpected error: {ex.Message}");
                }
            }
        }

        public async Task<OperationResult<List<Product>>> EditProductsAsync(List<Product> products, IClientSessionHandle session = null)
        {
            try
            {
                var updatedProducts = new List<Product>();
                var bulkUpdates = new List<WriteModel<Product>>();

                // Prepare bulk operations
                foreach (var product in products)
                {
                    var filter = Builders<Product>.Filter.Eq(p => p.Id, product.Id);
                    var update = Builders<Product>.Update
                        .Set(p => p.Id, product.Id)
                        .Set(p => p.Name, product.Name)
                        .Set(p => p.Quantity, product.Quantity)
                        .Set(p => p.Price, product.Price)
                        .Set(p => p.VendorId, product.VendorId)
                        .Set(p => p.CategoryId, product.CategoryId)
                        .Set(p => p.CategoryName, product.CategoryName);

                    bulkUpdates.Add(new UpdateOneModel<Product>(filter, update));
                }

                // Execute bulk operation
                BulkWriteResult<Product> result;
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
                    return OperationResult<List<Product>>.FailureResult(
                        404,
                        $"Some products were not updated. Expected {products.Count}, updated {result.ModifiedCount}");
                }

                // Fetch updated documents
                var updatedIds = products.Select(p => p.Id).ToList();
                var filterByIds = Builders<Product>.Filter.In(p => p.Id, updatedIds);

                updatedProducts = session == null
                    ? await _collection.Find(filterByIds).ToListAsync()
                    : await _collection.Find(session, filterByIds).ToListAsync();

                return OperationResult<List<Product>>.SuccessResult(updatedProducts);
            }
            catch (Exception ex)
            {
                switch (ex)
                {
                    case MongoCommandException mce when mce.Code == 11000:
                        return OperationResult<List<Product>>.FailureResult(409, "Duplicate key violation");

                    case TimeoutException:
                        return OperationResult<List<Product>>.FailureResult(504, "Database operation timed out");

                    case MongoException me:
                        return OperationResult<List<Product>>.FailureResult(500, $"Database error: {me.Message}");

                    default:
                        return OperationResult<List<Product>>.FailureResult(500, $"Unexpected error: {ex.Message}");
                }
            }
        }
    }
}
