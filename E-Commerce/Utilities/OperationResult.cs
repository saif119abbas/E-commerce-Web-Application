using E_Commerce.Models;

namespace E_Commerce.Utilities
{
    public class OperationResult<T>
    {
        public bool Success { get; private set; }
        public int StatusCode { get; private set; }
        public T? Data { get; private set; }
        public List<string> Errors { get; private set; } = new();

        private OperationResult() { }

        public static OperationResult<T> SuccessResult(T data, int statusCode=200)
            => new() { Success = true, Data = data,StatusCode=200 };

        public static OperationResult<T> FailureResult(int  statusCode,params string[] errors)
            => new() { Success = false, Errors = errors.ToList(),StatusCode=statusCode };
        public OperationResult<T> AddError(string error)
        {
            Errors.Add(error);
            return this;
        }
    }
}