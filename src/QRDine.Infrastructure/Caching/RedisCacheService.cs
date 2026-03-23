using QRDine.Application.Common.Abstractions.Caching;

namespace QRDine.Infrastructure.Caching
{
    public class RedisCacheService : ICacheService
    {
        private readonly IDistributedCache _redisCache;
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger<RedisCacheService> _logger;

        private static bool _isCircuitOpen = false;
        private static DateTime _nextRetryTime = DateTime.MinValue;
        private static readonly TimeSpan _cooldownTime = TimeSpan.FromSeconds(30);

        private static readonly TimeSpan _l1SurvivalDuration = TimeSpan.FromSeconds(60);

        public RedisCacheService(
            IDistributedCache redisCache,
            IMemoryCache memoryCache,
            ILogger<RedisCacheService> logger)
        {
            _redisCache = redisCache;
            _memoryCache = memoryCache;
            _logger = logger;
        }

        private bool IsRedisHealthy()
        {
            if (!_isCircuitOpen) return true;

            if (DateTime.UtcNow >= _nextRetryTime)
            {
                _isCircuitOpen = false;
                _logger.LogInformation("Circuit Breaker HALF-OPEN. Retrying Redis connection...");
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
                _logger.LogCritical(ex, "REDIS DOWN! Circuit TRIPPED on {Operation}. Fallback to L1 (MemoryCache) for the next {Seconds} seconds.", operation, _cooldownTime.TotalSeconds);
            }
        }

        public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
        {
            if (_memoryCache.TryGetValue(key, out T? l1Data))
            {
                return l1Data;
            }

            if (!IsRedisHealthy()) return default;

            try
            {
                var data = await _redisCache.GetStringAsync(key, cancellationToken);
                if (data == null) return default;

                var result = JsonSerializer.Deserialize<T>(data);

                _memoryCache.Set(key, result, TimeSpan.FromMinutes(1));
                return result;
            }
            catch (Exception ex)
            {
                TripCircuit(ex, "GetAsync");
                return default;
            }
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null, CancellationToken cancellationToken = default)
        {
            if (!IsRedisHealthy())
            {
                _logger.LogWarning("SURVIVAL MODE: Caching data to L1 (Memory) for key {Key}", key);
                _memoryCache.Set(key, value, _l1SurvivalDuration);
                return;
            }

            try
            {
                var options = new DistributedCacheEntryOptions();
                if (expiry.HasValue) options.SetAbsoluteExpiration(expiry.Value);

                var json = JsonSerializer.Serialize(value);
                await _redisCache.SetStringAsync(key, json, options, cancellationToken);

                _memoryCache.Set(key, value, expiry ?? _l1SurvivalDuration);
            }
            catch (Exception ex)
            {
                TripCircuit(ex, "SetAsync");

                _memoryCache.Set(key, value, _l1SurvivalDuration);
            }
        }

        public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
        {
            _memoryCache.Remove(key);

            if (!IsRedisHealthy()) return;

            try
            {
                await _redisCache.RemoveAsync(key, cancellationToken);
            }
            catch (Exception ex)
            {
                TripCircuit(ex, "RemoveAsync");
            }
        }

        public async Task RemoveMultipleAsync(IEnumerable<string> keys, CancellationToken cancellationToken = default)
        {
            if (keys == null) return;

            foreach (var key in keys)
            {
                _memoryCache.Remove(key);
            }

            if (!IsRedisHealthy()) return;

            try
            {
                var tasks = keys.Select(key => _redisCache.RemoveAsync(key, cancellationToken));
                await Task.WhenAll(tasks);
            }
            catch (Exception ex)
            {
                TripCircuit(ex, "RemoveMultipleAsync");
            }
        }
    }
}
