using E_Commerce.Configuration;
using E_Commerce.Utilities;
using Elasticsearch.Net;
using Microsoft.Extensions.Options;
using Nest;
using Polly;
using Polly.Retry;

namespace E_Commerce.Services
{
    public class ElasticsearchService<T> : IElasticsearchService<T> where T : class
    {
        private readonly ElasticClient _elasticClient;
        private readonly ILogger<ElasticsearchService<T>> _logger;
        private readonly AsyncRetryPolicy _retryPolicy;
        private readonly ElasticSettings _elasticSettings;

        public ElasticsearchService(
            ElasticClient elasticClient,
            ILogger<ElasticsearchService<T>> logger,
            IOptions<ElasticSettings> elasticSettings
            )
        {
            _elasticClient = elasticClient;
            _logger = logger;

     
            _retryPolicy = Polly.Policy
                    .Handle<ElasticsearchClientException>()
                   .Or<HttpRequestException>()
                   .WaitAndRetryAsync(
                    retryCount: 3,
                    sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
                    onRetry: (ex, delay, attempt, _) =>
                        _logger.LogWarning(ex, $"Retry {attempt} for Elasticsearch operation."));
            _elasticSettings = elasticSettings.Value;
        }

  
        public async Task<OperationResult<T>> CreateAsync(T entity)
        {
            try
            {
                if (entity == null)
                    return OperationResult<T>.FailureResult(400, "Entity cannot be null.");

                var response = await _retryPolicy.ExecuteAsync(async () =>
                    await _elasticClient.IndexDocumentAsync(entity));

                if (!response.IsValid)
                    return HandleElasticsearchError(response);

                return OperationResult<T>.SuccessResult(entity);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

 
        public async Task<OperationResult<T>> GetAsync(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                    return OperationResult<T>.FailureResult(400, "ID cannot be empty.");

                var response = await _retryPolicy.ExecuteAsync(async () =>
                    await _elasticClient.GetAsync(new DocumentPath<T>(id)));

                if (!response.IsValid)
                    return HandleElasticsearchError(response);

                if (!response.Found)
                    return OperationResult<T>.FailureResult(404, "Document not found.");

                return OperationResult<T>.SuccessResult(response.Source);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        public async Task<OperationResult<List<T>>> GetAllAsync()
        {
            try
            {
                var response = await _retryPolicy.ExecuteAsync(async () =>
                    await _elasticClient.SearchAsync<T>(s => s.MatchAll().Size(10000)));

                if (!response.IsValid)
                    return OperationResult<List<T>>.FailureResult(500,
                        response.ServerError?.Error?.Reason ?? "Failed to fetch documents.");

                return OperationResult<List<T>>.SuccessResult(response.Documents.ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch all documents.");
                return OperationResult<List<T>>.FailureResult(500, ex.Message);
            }
        }

        public async Task<OperationResult<T>> UpdateAsync(T entity)
        {
            try
            {
                if (entity == null)
                    return OperationResult<T>.FailureResult(400, "Entity cannot be null.");

                var response = await _retryPolicy.ExecuteAsync(async () =>
                    await _elasticClient.UpdateAsync(new DocumentPath<T>(entity), u => u.Doc(entity).RetryOnConflict(3)));

                if (!response.IsValid)
                    return HandleElasticsearchError(response);

                return OperationResult<T>.SuccessResult(entity);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        public async Task<OperationResult<T>> DeleteAsync(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                    return OperationResult<T>.FailureResult(400, "ID cannot be empty.");

                var response = await _retryPolicy.ExecuteAsync(async () =>
                    await _elasticClient.DeleteAsync( new DocumentPath<T>(id)));

                if (!response.IsValid)
                    return HandleElasticsearchError(response);

                if (response.Result == Result.NotFound)
                    return OperationResult<T>.FailureResult(404, "Document not found.");
                return OperationResult<T>.SuccessResult(null);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }
        public async Task<OperationResult<bool>> BulkUpdateAsync(IEnumerable<T> entities)
        {
            try
            {
                if (entities == null || !entities.Any())
                    return OperationResult<bool>.FailureResult(400, "Entities cannot be null or empty.");

                var response = await _elasticClient.BulkAsync(b => b.Index(_elasticSettings.Index).
                UpdateMany(entities, (ed, e) => ed.Doc(e).DocAsUpsert(true)));

                return OperationResult<bool>.SuccessResult(true);
            }
            catch (Exception ex)
            {
                return OperationResult<bool>.FailureResult(500, $"Bulk update failed: {ex.Message}");
            }
        }

        private OperationResult<bool> HandleBulkError(BulkResponse response)
        {
            var errors = response.ItemsWithErrors.Select(i => i.Error.Reason);
            _logger.LogError("Bulk operation errors: {Errors}", string.Join(", ", errors));
            return OperationResult<bool>.FailureResult(500, $"Partial failures: {response.ItemsWithErrors.Count()}/{response.Items.Count}");
        }
        private OperationResult<T> HandleElasticsearchError(ResponseBase response)
        {
            string error = response.ServerError?.Error?.Reason ?? "Elasticsearch operation failed.";
            _logger.LogError("Elasticsearch error: {Error}", error);
            return OperationResult<T>.FailureResult(500, error);
        }

        private OperationResult<T> HandleException(Exception ex)
        {
            _logger.LogError(ex, "Elasticsearch service error");

            return ex switch
            {
                ElasticsearchClientException =>
                    OperationResult<T>.FailureResult(503, "Elasticsearch unavailable."),
                TimeoutException =>
                    OperationResult<T>.FailureResult(504, "Request timed out."),
                _ =>
                    OperationResult<T>.FailureResult(500, $"Unexpected error: {ex.Message}")
            };
        }
        public async Task<OperationResult<List<T>>> SearchAsync(string searchQuery)
        {
            try
            {
                if (string.IsNullOrEmpty(searchQuery))
                    return OperationResult<List<T>>.FailureResult(400, "Search query cannot be empty.");

                var response = await _elasticClient.SearchAsync<T>(s => s
                    .Query(q => q
                        .MultiMatch(mm => mm
                            .Query(searchQuery)
                            .Fields(f => f
                                .Field("*")
                            )
                            .Fuzziness(Fuzziness.Auto)
                        )
                    )
                    .Size(100)
                );

                if (!response.IsValid)
                    return OperationResult<List<T>>.FailureResult(500,
                        response.ServerError?.Error?.Reason ?? "Search operation failed");

                if (!response.Documents.Any())
                    return OperationResult<List<T>>.FailureResult(404, "No documents found matching the query");

                return OperationResult<List<T>>.SuccessResult(response.Documents.ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Search operation failed");
                return OperationResult<List<T>>.FailureResult(500, ex.Message);
            }
        }


    }
}