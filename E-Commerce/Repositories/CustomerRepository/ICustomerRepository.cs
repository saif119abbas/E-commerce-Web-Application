using E_Commerce.Models;
using E_Commerce.Utilities;
using MongoDB.Driver;
namespace E_Commerce.Repositories
{
    public interface ICustomerRepository
    {
        Task<OperationResult<Customer>> CreateCustomerAsync(Customer model,IClientSessionHandle session=null);
        Task<OperationResult<Customer>> GetCustomerByIdAsync(Guid UserId);

    }
}
