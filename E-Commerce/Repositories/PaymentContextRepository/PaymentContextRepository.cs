using E_Commerce.Models;
using E_Commerce.Utilities;
using MongoDB.Driver;

namespace E_Commerce.Repositories
{
    public class PaymentContextRepository : MongoRepository<PaymentContext>, IPaymentContextRepository
    {
        public PaymentContextRepository(IMongoDatabase database, IUnitOfWork unitOfWork) :
       base(database, unitOfWork, "payment_contexts")
        {
        }
        public async Task<OperationResult<PaymentContext>> CreateAsync(PaymentContext context, IClientSessionHandle session = null)
        {
            try
            {
                if (session == null)
                {

                    await _collection.InsertOneAsync(context);
                }
                else
                {
                    await _collection.InsertOneAsync(session, context);
                }

                return OperationResult<PaymentContext>.SuccessResult(context);
            }
            catch(Exception ex)
            {
                switch (ex)
                {
                    case MongoWriteException mwe when mwe.WriteError.Category == ServerErrorCategory.DuplicateKey:
                        return OperationResult<PaymentContext>.FailureResult(409, "Product with this ID already exists");

                    case MongoCommandException mce when mce.Code == 11000:
                        return OperationResult<PaymentContext>.FailureResult(409, "Duplicate key violation");


                    case TimeoutException:
                        return OperationResult<PaymentContext>.FailureResult(504, "Database operation timed out");


                    case MongoException me:
                        return OperationResult<PaymentContext>.FailureResult(500, $"Database error: {me.Message}");


                    default:
                        return OperationResult<PaymentContext>.FailureResult(500, $"Unexpected error: {ex.Message}");
                }
            }
         
         
        }

        public async Task<OperationResult<PaymentContext>> DeleteAsync(Guid contextId, IClientSessionHandle session = null)
        {
            try
            {

                var deleteResult = session == null ? await _collection.DeleteOneAsync(p => p.Id==contextId) :
                                                await _collection.DeleteOneAsync(session, p => p.Id == contextId);

                if (deleteResult.DeletedCount == 0)
                {
                    return OperationResult<PaymentContext>.FailureResult(404, "Product not found or already deleted");
                }

                return OperationResult<PaymentContext>.SuccessResult(null);
            }
            catch (Exception ex)
            {
                switch (ex)
                {
                    case MongoCommandException mce when mce.Code == 11000:
                        return OperationResult<PaymentContext>.FailureResult(409, "Duplicate key violation");

                    case TimeoutException:
                        return OperationResult<PaymentContext>.FailureResult(504, "Database operation timed out");

                    case MongoException me:
                        return OperationResult<PaymentContext>.FailureResult(500, $"Database error: {me.Message}");

                    default:
                        return OperationResult<PaymentContext>.FailureResult(500, $"Unexpected error: {ex.Message}");
                }
            }
        }

        public async Task<OperationResult<PaymentContext>> DeletePaymentContextAsync(Guid contextId, IClientSessionHandle session = null)
        {
            try
            {

                var deleteResult = session == null ? await _collection.DeleteOneAsync(c=>c.Id==contextId) :
                                                await _collection.DeleteOneAsync(session, c=> c.Id == contextId);

                if (deleteResult.DeletedCount == 0)
                {
                    return OperationResult<PaymentContext>.FailureResult(404, "Product not found or already deleted");
                }

                return OperationResult<PaymentContext>.SuccessResult(null);
            }
            catch (Exception ex)
            {
                switch (ex)
                {
                    case MongoCommandException mce when mce.Code == 11000:
                        return OperationResult<PaymentContext>.FailureResult(409, "Duplicate key violation");

                    case TimeoutException:
                        return OperationResult<PaymentContext>.FailureResult(504, "Database operation timed out");

                    case MongoException me:
                        return OperationResult<PaymentContext>.FailureResult(500, $"Database error: {me.Message}");

                    default:
                        return OperationResult<PaymentContext>.FailureResult(500, $"Unexpected error: {ex.Message}");
                }
            }
        }

        public async Task<OperationResult<PaymentContext>> GetAsync(string sessionId)
        {
            try
            {

                PaymentContext result = await _collection.Find(p => p.StripeSessionId== sessionId).FirstOrDefaultAsync();
                if (result == null)
                {
                    return OperationResult<PaymentContext>.FailureResult(404, "The product doesn't exsit");
                }

                return OperationResult<PaymentContext>.SuccessResult(result);
            }
            catch (Exception ex)
            {
                switch (ex)
                {
                    case MongoCommandException mce when mce.Code == 11000:
                        return OperationResult<PaymentContext>.FailureResult(409, "Duplicate key violation");

                    case TimeoutException:
                        return OperationResult<PaymentContext>.FailureResult(504, "Database operation timed out");

                    case MongoException me:
                        return OperationResult<PaymentContext>.FailureResult(500, $"Database error: {me.Message}");

                    default:

                        return OperationResult<PaymentContext>.FailureResult(500, $"Unexpected error: {ex.Message}");
                }
            }
        }
    }
}
