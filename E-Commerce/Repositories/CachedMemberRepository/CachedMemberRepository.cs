using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;

namespace E_Commerce.Repositories
{
    public class CachedMemberRepository<T> : IMemberRepository<T> where T : class
    {
        private readonly IDistributedCache _distributedCache;

        public CachedMemberRepository(IDistributedCache distributedCache)
        {
            _distributedCache = distributedCache;
        }

        public async Task<T> AddMemberAsync(string key, T member, CancellationToken token = default)
        {
            var options = new DistributedCacheEntryOptions
            {
              
                 AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30)
            };

            await _distributedCache.SetStringAsync(
                key,
                JsonConvert.SerializeObject(member),
                options,
                token);

            return member;
        }
        public async Task<List<T>> AddMemberListAsync(string key, List<T> members, CancellationToken token = default)
        {
            var options = new DistributedCacheEntryOptions
            {
                // Set cache expiration if needed
                 AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30)
            };

            await _distributedCache.SetStringAsync(
                key,
                JsonConvert.SerializeObject(members),
                options,
                token);

            return members;
        }

        public async Task<T> GetMemberAsync(string key, CancellationToken token = default)
        {
            var cachedMember = await _distributedCache.GetStringAsync(key, token);

            if (string.IsNullOrEmpty(cachedMember))
                return null!;

            return JsonConvert.DeserializeObject<T>(cachedMember,
                new JsonSerializerSettings
                {
                    ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor
                })!;
        }

        public async Task<List<T>> GetMemberListAsync(string key, CancellationToken token = default)
        {
            var cachedList = await _distributedCache.GetStringAsync(key, token);

            if (string.IsNullOrEmpty(cachedList))
                return new List<T>();

            return JsonConvert.DeserializeObject<List<T>>(cachedList,
                new JsonSerializerSettings
                {
                    ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor
                })!;
        }

        public async Task RemoveMemberAsync(string key, CancellationToken token = default)
        {
            await _distributedCache.RemoveAsync(key, token);
        }
        public async Task<T> UpdateMemberAsync(string key, T member, CancellationToken token = default)
        {
            // Check if key exists first
            var existing = await _distributedCache.GetStringAsync(key, token);
            if (existing == null)
            {
                throw new KeyNotFoundException($"Cache key {key} not found");
            }

            var options = new DistributedCacheEntryOptions
            {
                // Preserve original expiration if needed
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30)
            };

            await _distributedCache.SetStringAsync(key, JsonConvert.SerializeObject(member), options, token);
            return member;
        }

        public async Task<List<T>> UpdateMemberListAsync(string key, List<T> members, CancellationToken token = default)
        {
            // Check if key exists
            var existing = await _distributedCache.GetStringAsync(key, token);
            if (existing == null)
            {
                throw new KeyNotFoundException($"Cache key {key} not found");
            }

            var options = new DistributedCacheEntryOptions
            {
                // Keep original expiration
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30)
            };

            await _distributedCache.SetStringAsync(key, JsonConvert.SerializeObject(members), options, token);
            return members;
        }
    }
}