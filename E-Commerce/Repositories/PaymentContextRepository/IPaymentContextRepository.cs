using E_Commerce.Models;
using E_Commerce.Utilities;
using MongoDB.Driver;

namespace E_Commerce.Repositories
{
    public interface IPaymentContextRepository
    {
        Task<OperationResult<PaymentContext>> CreateAsync(PaymentContext context, IClientSessionHandle session = null);
        Task<OperationResult<PaymentContext>> DeleteAsync(Guid contextId, IClientSessionHandle session = null);
        Task<OperationResult<PaymentContext>> GetAsync(string sessionId);
        Task<OperationResult<PaymentContext>> DeletePaymentContextAsync(Guid contextId, IClientSessionHandle session = null);
    }
}
