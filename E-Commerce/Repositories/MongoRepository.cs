using E_Commerce.Repositories;
using MongoDB.Driver;


public abstract class MongoRepository<TEntity> where TEntity : class 
{
    protected readonly IMongoCollection<TEntity> _collection;
    protected readonly IUnitOfWork _unitOfWork;

    protected MongoRepository(
        IMongoDatabase database,
        IUnitOfWork unitOfWork,
        string collectionName)
    {
        _collection = database.GetCollection<TEntity>(collectionName);
        _unitOfWork = unitOfWork;
    }

    protected IClientSessionHandle Session =>
        _unitOfWork?.Session ?? throw new InvalidOperationException("Unit of Work not initialized");
}