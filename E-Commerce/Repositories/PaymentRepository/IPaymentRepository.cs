using E_Commerce.Models;
using E_Commerce.Utilities;
using MongoDB.Driver;

namespace E_Commerce.Repositories
{
    public interface IPaymentRepository
    {
        Task<OperationResult<Payment>> CreatePaymentAsync(Payment payment, IClientSessionHandle session = null);
        Task<OperationResult<Payment>> GetPaymentAsync(Guid Id);
        Task<OperationResult<Payment>> UpdatePaymentAsync(Payment payment, IClientSessionHandle session = null);
        Task<OperationResult<List<Payment>>> GetCustomerPaymentsAsync(Guid customerId, int limit = 50);
    }
}
