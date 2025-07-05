using E_Commerce.Utilities;

namespace E_Commerce.Services
{
    public interface IElasticsearchService<T> where T : class
    {
        Task<OperationResult<T>> CreateAsync(T  entity);
        Task<OperationResult<List<T>>> SearchAsync(string searchQuery);
        Task<OperationResult<T>> GetAsync(string id);
        Task<OperationResult<List<T>>> GetAllAsync();
        Task<OperationResult<T>> UpdateAsync(T entity);
        Task<OperationResult<T>> DeleteAsync(string id);
        Task<OperationResult<bool>> BulkUpdateAsync(IEnumerable<T> entities);
    }
}
