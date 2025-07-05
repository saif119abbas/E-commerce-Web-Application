using E_Commerce.Models;
using E_Commerce.Utilities;
using MongoDB.Driver;

namespace E_Commerce.Repositories.CustomerRepository
{
    public class CustomerRepository : MongoRepository<Customer>, ICustomerRepository
    {
    
        public CustomerRepository(IMongoDatabase database, IUnitOfWork unitOfWork)
         : base(database, unitOfWork, "Customers")
        {
            
        }
        public async Task<OperationResult<Customer>> CreateCustomerAsync(Customer model, IClientSessionHandle session = null)
        {
            try
            {

                if (session == null)
                {
                    await _collection.InsertOneAsync(model);
                }
                else
                {
                    await _collection.InsertOneAsync(session,model);
                }
                return OperationResult<Customer>.SuccessResult(model);
            }
            catch (MongoWriteException ex) when (ex.WriteError.Category == ServerErrorCategory.DuplicateKey)
            {
                return OperationResult<Customer>.FailureResult(409, "Customer with this ID already exists");

            }
            catch (MongoCommandException ex) when (ex.Code == 11000)
            {

                return OperationResult<Customer>.FailureResult(409, "Duplicate key violation");
            }
            catch (TimeoutException ex)
            {

                return OperationResult<Customer>.FailureResult(504, "Database operation timed out");
            }
            catch (MongoException ex)
            {

                return OperationResult<Customer>.FailureResult(500, $"Database error: {ex.Message}");
            }
            catch (Exception ex)
            {
                return OperationResult<Customer>.FailureResult(500, $"Unexpected error: {ex.Message}");
            }

        }

        public async Task<OperationResult<Customer>> GetCustomerByIdAsync(Guid UserId)
        {
            try
            {
                var customer=await findCustomer(UserId);
                return OperationResult<Customer>.SuccessResult(customer);
            }
            catch (Exception ex)
            {
                switch (ex)
                {
                    case MongoCommandException mce when mce.Code == 11000:
                        return OperationResult<Customer>.FailureResult(409, "Duplicate key violation");

                    case TimeoutException:
                        return OperationResult<Customer>.FailureResult(504, "Database operation timed out");

                    case MongoException me:
                        return OperationResult<Customer>.FailureResult(500, $"Database error: {me.Message}");

                    default:
                        return OperationResult<Customer>.FailureResult(500, $"Unexpected error: {ex.Message}");
                }
            }
        }
        private async Task<Customer> findCustomer(Guid customerId) =>
          await _collection.Find(c => c.UserId == customerId).FirstOrDefaultAsync();
    }
}
