using System.Linq.Expressions;
using E_Commerce.Utilities;
using MongoDB.Driver;

namespace E_Commerce.Repositories
{
    public interface IRepository<TEntity> where TEntity : class
    {
        Task<OperationResult<IEnumerable<List<TEntity>>>> GetAllAsync();
        Task<OperationResult<TEntity>> GetByIdAsync(Guid id);
        Task<OperationResult<TEntity>> AddAsync(TEntity entity, IClientSessionHandle session = null);
        Task<OperationResult<TEntity>> UpdateAsync(string id, TEntity entity, IClientSessionHandle session = null);
        Task<OperationResult<TEntity>> DeleteAsync(string id, IClientSessionHandle session = null);
    }

}
