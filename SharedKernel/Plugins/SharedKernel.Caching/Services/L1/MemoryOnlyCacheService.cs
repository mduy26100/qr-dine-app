namespace SharedKernel.Caching.Services.L1
{
    public class MemoryOnlyCacheService : ICacheService
    {
        private readonly IMemoryCache _memoryCache;

        public MemoryOnlyCacheService(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
        {
            _memoryCache.TryGetValue(key, out T? value);
            return Task.FromResult(value);
        }

        public Task SetAsync<T>(string key, T value, TimeSpan? expiry = null, CancellationToken cancellationToken = default)
        {
            if (expiry.HasValue)
            {
                _memoryCache.Set(key, value, expiry.Value);
            }
            else
            {
                _memoryCache.Set(key, value);
            }
            return Task.CompletedTask;
        }

        public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
        {
            _memoryCache.Remove(key);
            return Task.CompletedTask;
        }

        public Task RemoveMultipleAsync(IEnumerable<string> keys, CancellationToken cancellationToken = default)
        {
            if (keys != null)
            {
                foreach (var key in keys)
                {
                    _memoryCache.Remove(key);
                }
            }
            return Task.CompletedTask;
        }
    }
}
