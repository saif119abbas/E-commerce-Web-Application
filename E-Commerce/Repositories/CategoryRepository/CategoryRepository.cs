using E_Commerce.Models;
using E_Commerce.Utilities;
using MongoDB.Driver;

namespace E_Commerce.Repositories
{
    public class CategoryRepository : MongoRepository<Category>, ICategoryRepository
    {

        public CategoryRepository(IMongoDatabase database, IUnitOfWork unitOfWork)
         : base(database, unitOfWork, "Categories")
        {


            try
            {
                var indexKeys = Builders<Category>.IndexKeys.Ascending(c => c.Name);
                var indexOptions = new CreateIndexOptions { Unique = true };
                var indexModel = new CreateIndexModel<Category>(indexKeys, indexOptions);
                _collection.Indexes.CreateOne(indexModel);
            }
            catch (MongoException ex)
            {
                // Log index creation error
                Console.WriteLine($"Index creation error: {ex.Message}");
            }
        }

        public async Task<OperationResult<Category>> AddCategoryAsync(Category category, IClientSessionHandle session = null)
        {

            category.Id = Guid.NewGuid();
            await _collection.InsertOneAsync(category);
            return OperationResult<Category>.SuccessResult(category);
        }

        public async Task<OperationResult<Category>> GetCategoryByIdAsync(string id)
        {
            if (!Guid.TryParse(id, out Guid categoryId))
                return OperationResult<Category>.FailureResult(500, "Invalid product ID format.");
            var category = await findCategory(categoryId);

            return OperationResult<Category>.SuccessResult(category);

        }

        public async Task<OperationResult<Category>> GetCategoryByNameAsync(string name)
        {
            if (name == null)
                return OperationResult<Category>.FailureResult(500, "Not a valid category");
            var category = await _collection.Find(c => c.Name == name).FirstOrDefaultAsync();
            if (category == null)
                return OperationResult<Category>.FailureResult(400, "Not exsist category");
            return OperationResult<Category>.SuccessResult(category);
        }
        public async Task<bool> UpdateCategoryAsync(Category category)
        {
            var filter = Builders<Category>.Filter.Eq(c => c.Id, category.Id);
            var updateResult = await _collection.ReplaceOneAsync(filter, category);
            return updateResult.IsAcknowledged && updateResult.ModifiedCount > 0;
        }
        public async Task<OperationResult<List<Category>>> GetAllCategoriesAsync()
        {
            try
            {

                var categories = await _collection.Find((c) => true).ToListAsync();
                if (categories.Count == 0)
                    return OperationResult<List<Category>>.FailureResult(404, "Theres no categories");
                return OperationResult<List<Category>>.SuccessResult(categories);

            }
            catch (Exception ex)
            {
                switch (ex)
                {
                    case MongoCommandException mce when mce.Code == 11000:
                        return OperationResult<List<Category>>.FailureResult(409, "Duplicate key violation");

                    case TimeoutException:
                        return OperationResult<List<Category>>.FailureResult(504, "Database operation timed out");

                    case MongoException me:
                        return OperationResult<List<Category>>.FailureResult(500, $"Database error: {me.Message}");

                    default:
                        return OperationResult<List<Category>>.FailureResult(500, $"Unexpected error: {ex.Message}");
                }
            }

        }

        /* public async Task<OperationResult<Category>> AssignProductToCategoryAsync(Product product, Guid categoryId)
         {
             var category = await findCategory(categoryId);
             if (category == null)
             {
                 return OperationResult<Category>.FailureResult(404, "Category not found.");
             }

             var found =  findProduct(product.Id, category);
             if (!found)
             {
                 category.Products.Add(product);
                 var updateResult = await UpdateCategoryAsync(category);
                 if (!updateResult)
                 {
                     return OperationResult<Category>.FailureResult(500, "Failed to update category.");
                 }
                 return OperationResult<Category>.SuccessResult(category);
             }

             return OperationResult<Category>.FailureResult(409, "The product is already added");
         }


         /*public async Task<OperationResult<Category>> RemoveProduct(Guid productId, Guid categoryId)
         {
             try
             {
                 var update = Builders<Category>.Update.PullFilter(
                     c => c.Products,
                     Builders<Product>.Filter.Eq(p => p.Id, productId)
                 );

                 var result = await _collection.UpdateOneAsync(
                     c => c.Id == categoryId,
                     update
                 );

                 if (result.ModifiedCount == 0 || !result.IsAcknowledged)
                 {
                     return OperationResult<Category>.FailureResult(404,
                         "Product not found in the specified category or already removed");
                 }

                 return OperationResult<Category>.SuccessResult(null);
             }
             catch (Exception ex)
             {
                 return ex switch
                 {
                     MongoCommandException mce when mce.Code == 11000 =>
                         OperationResult<Category>.FailureResult(409, "Duplicate key violation"),

                     TimeoutException =>
                         OperationResult<Category>.FailureResult(504, "Database operation timed out"),

                     MongoException me =>
                         OperationResult<Category>.FailureResult(500, $"Database error: {me.Message}"),

                     _ =>
                         OperationResult<Category>.FailureResult(500, $"Unexpected error: {ex.Message}")
                 };
             }
         }

         private bool findProduct(Guid productId, Category category)=> 
             category.Products.Find(p => p.Id == productId) == null ? false : true;*/

        private async Task<Category> findCategory(Guid categoryId) =>
          await _collection.Find(c => c.Id == categoryId).FirstOrDefaultAsync();



    } 
}