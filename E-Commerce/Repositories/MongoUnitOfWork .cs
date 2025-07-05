using E_Commerce.Repositories;
using MongoDB.Driver;

public class MongoUnitOfWork : IUnitOfWork, IDisposable
{
    private readonly IMongoClient _mongoClient;
    private IClientSessionHandle _session;
    private bool _disposed = false;
    private bool _transactionStarted = false;

    public MongoUnitOfWork(IMongoClient mongoClient)
    {
        _mongoClient = mongoClient;
    }

    public IClientSessionHandle Session
    {
        get
        {
            if (_session == null || !_transactionStarted)
                throw new InvalidOperationException("Transaction not started");
            return _session;
        }
    }

    public async Task StartTransactionAsync()
    {
        if (_session != null)
            throw new InvalidOperationException("Transaction already started");

        _session = await _mongoClient.StartSessionAsync();
        _session.StartTransaction(new TransactionOptions(
            readConcern: ReadConcern.Snapshot,
            writeConcern: WriteConcern.WMajority));
        _transactionStarted = true;
    }

    public async Task CommitAsync()
    {
        if (_session == null || !_transactionStarted)
            throw new InvalidOperationException("No transaction to commit");

        try
        {
            await _session.CommitTransactionAsync();
            _transactionStarted = false;
        }
        catch
        {
            await AbortAsync();
            throw;
        }
    }

    public async Task AbortAsync()
    {
        if (_session == null || !_transactionStarted) return;

        try
        {
            await _session.AbortTransactionAsync();
        }
        finally
        {
            _transactionStarted = false;
            Cleanup();
        }
    }

    private void Cleanup()
    {
        _session?.Dispose();
        _session = null;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                Cleanup();
            }
            _disposed = true;
        }
    }
}