namespace E_Commerce.Repositories
{
    public interface IMemberRepository<T> where T : class
    {
        public Task<T> GetMemberAsync(string key, CancellationToken token = default);
        Task<List<T>> GetMemberListAsync(string key, CancellationToken token = default);
        public Task<T> AddMemberAsync(string key,T member, CancellationToken token=default);
        public Task<List<T>> AddMemberListAsync(string key, List<T> members, CancellationToken token = default);
        Task<T> UpdateMemberAsync(string key, T member, CancellationToken token = default);
        Task<List<T>> UpdateMemberListAsync(string key, List<T> members, CancellationToken token = default);
    }
}
