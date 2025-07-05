using E_Commerce.Repositories;
using MongoDB.Driver;

public abstract class MongoRepository<T> where T : class
{
    protected readonly IMongoCollection<T> _collection;
    protected readonly IUnitOfWork _unitOfWork;

    protected MongoRepository(
        IMongoDatabase database,
        IUnitOfWork unitOfWork,
        string collectionName)
    {
        _collection = database.GetCollection<T>(collectionName);
        _unitOfWork = unitOfWork;
    }

    protected IClientSessionHandle Session =>
        _unitOfWork?.Session ?? throw new InvalidOperationException("Unit of Work not initialized");
}