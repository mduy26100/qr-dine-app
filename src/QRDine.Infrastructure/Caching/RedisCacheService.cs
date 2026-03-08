using QRDine.Application.Common.Abstractions.Caching;

namespace QRDine.Infrastructure.Caching
{
    public class RedisCacheService : ICacheService
    {
        private readonly IDistributedCache _cache;
        private readonly ILogger<RedisCacheService> _logger;

        private static bool _isCircuitOpen = false;
        private static DateTime _nextRetryTime = DateTime.MinValue;
        private static readonly TimeSpan _cooldownTime = TimeSpan.FromSeconds(30);

        public RedisCacheService(IDistributedCache cache, ILogger<RedisCacheService> logger)
        {
            _cache = cache;
            _logger = logger;
        }

        private bool CanUseCache()
        {
            if (!_isCircuitOpen) return true;

            if (DateTime.UtcNow >= _nextRetryTime)
            {
                _isCircuitOpen = false;
                return true;
            }

            return false;
        }

        private void TripCircuit(Exception ex, string operation)
        {
            if (!_isCircuitOpen)
            {
                _isCircuitOpen = true;
                _nextRetryTime = DateTime.UtcNow.Add(_cooldownTime);
                _logger.LogCritical(ex, "REDIS DOWN! Circuit Breaker TRIPPED on {Operation}. Bypassing cache for the next {Seconds} seconds.", operation, _cooldownTime.TotalSeconds);
            }
        }

        public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
        {
            if (!CanUseCache()) return default;

            try
            {
                var data = await _cache.GetStringAsync(key, cancellationToken);
                return data == null ? default : JsonSerializer.Deserialize<T>(data);
            }
            catch (Exception ex)
            {
                TripCircuit(ex, "GetAsync");
                return default;
            }
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null, CancellationToken cancellationToken = default)
        {
            if (!CanUseCache()) return;

            try
            {
                var options = new DistributedCacheEntryOptions();
                if (expiry.HasValue) options.SetAbsoluteExpiration(expiry.Value);

                var json = JsonSerializer.Serialize(value);
                await _cache.SetStringAsync(key, json, options, cancellationToken);
            }
            catch (Exception ex)
            {
                TripCircuit(ex, "SetAsync");
            }
        }

        public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
        {
            if (!CanUseCache()) return;

            try
            {
                await _cache.RemoveAsync(key, cancellationToken);
            }
            catch (Exception ex)
            {
                TripCircuit(ex, "RemoveAsync");
            }
        }
    }
}
