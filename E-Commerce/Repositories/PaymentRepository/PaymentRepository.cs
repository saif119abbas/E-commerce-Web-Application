
using E_Commerce.Models;
using E_Commerce.Utilities;
using MongoDB.Driver;

namespace E_Commerce.Repositories.PaymentRepository
{
    public class PaymentRepository : MongoRepository<Payment>, IPaymentRepository
    {
        public PaymentRepository(IMongoDatabase database, IUnitOfWork unitOfWork) :
          base(database, unitOfWork, "Payments")
        {
        }

        public async Task<OperationResult<Payment>> CreatePaymentAsync(Payment payment,IClientSessionHandle session=null)
        {
            try
            {
                if (session == null)
                {
                    await _collection.InsertOneAsync(payment);
                }
                else
                {
                    await _collection.InsertOneAsync(session, payment);
                }
                return OperationResult<Payment>.SuccessResult(payment);
            }
            catch (Exception ex)
            {
                switch (ex)
                {
                    case MongoWriteException mwe when mwe.WriteError.Category == ServerErrorCategory.DuplicateKey:
                    case MongoCommandException mce when mce.Code == 11000:
                        return OperationResult<Payment>.FailureResult(409, "Duplicate key violation");

                    case TimeoutException:
                        return OperationResult<Payment>.FailureResult(504, "Database operation timed out");

                    case MongoException me:
                        return OperationResult<Payment>.FailureResult(500, $"Database error: {me.Message}");

                    default:
                        return OperationResult<Payment>.FailureResult(500, $"Unexpected error: {ex.Message}");
                }
            }
        }

        public Task<OperationResult<Payment>> GetPaymentAsync(Guid Id)
        {
            throw new NotImplementedException();
        }
        public Task<OperationResult<Payment>> UpdatePaymentAsync(Payment payment, IClientSessionHandle session = null)
        {
            throw new NotImplementedException();
        }

        public Task<OperationResult<List<Payment>>> GetCustomerPaymentsAsync(Guid customerId, int limit = 50)
        {
            throw new NotImplementedException();
        }

     
    }
}
