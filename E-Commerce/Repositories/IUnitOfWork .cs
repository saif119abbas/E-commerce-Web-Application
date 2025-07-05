using MongoDB.Driver;

namespace E_Commerce.Repositories
{
    public interface IUnitOfWork: IDisposable
    {
        IClientSessionHandle Session { get; }
        Task StartTransactionAsync();
        Task CommitAsync();
        Task AbortAsync();
    }
}
