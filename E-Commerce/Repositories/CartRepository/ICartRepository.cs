using E_Commerce.Models;
using E_Commerce.Utilities;
using MongoDB.Driver;
namespace E_Commerce.Repositories
{
    public interface ICartRepository
    {
        Task<OperationResult<Cart>> AddCartAsync(Cart cart, IClientSessionHandle session = null);
        Task<OperationResult<Cart>> UpdateCartAsync(Cart cart, IClientSessionHandle session = null);
        Task<OperationResult<Cart>> GetCartAsync(Guid  userId);
    }
}
