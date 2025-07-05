namespace E_Commerce.Repositories
{
    public interface IRepository<TEntity> where TEntity : class
    {
        Task<IEnumerable<TEntity>> GetAllAsync();
        Task<TEntity> GetByIdAsync(string id);
        Task AddAsync(TEntity entity);
        Task UpdateAsync(string id, TEntity entity);
        Task DeleteAsync(string id);
    }

}
