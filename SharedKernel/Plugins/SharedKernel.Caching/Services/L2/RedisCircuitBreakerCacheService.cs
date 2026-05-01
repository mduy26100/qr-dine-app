namespace SharedKernel.Caching.Services.L2
{
    public class RedisCircuitBreakerCacheService : ICacheService
    {
        private readonly IDistributedCache _redisCache;
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger<RedisCircuitBreakerCacheService> _logger;
        private readonly RedisSettings _settings;

        private enum CircuitState { Closed, Open, HalfOpen }
        private static CircuitState _circuitState = CircuitState.Closed;
        private static DateTime _nextRetryTime = DateTime.MinValue;
        private static readonly object _circuitLock = new();

        public RedisCircuitBreakerCacheService(
            IDistributedCache redisCache,
            IMemoryCache memoryCache,
            ILogger<RedisCircuitBreakerCacheService> logger,
            IOptions<RedisSettings> options)
        {
            _redisCache = redisCache;
            _memoryCache = memoryCache;
            _logger = logger;
            _settings = options.Value;
        }

        private TimeSpan CooldownTime => TimeSpan.FromSeconds(_settings.CircuitBreakerCooldownSeconds);
        private TimeSpan L1SurvivalDuration => TimeSpan.FromSeconds(_settings.L1SurvivalDurationSeconds);

        private bool IsRedisHealthy()
        {
            if (_circuitState == CircuitState.Closed) return true;

            if (_circuitState == CircuitState.Open)
            {
                if (DateTime.UtcNow >= _nextRetryTime)
                {
                    lock (_circuitLock)
                    {
                        if (_circuitState == CircuitState.Open)
                        {
                            _circuitState = CircuitState.HalfOpen;
                            _logger.LogWarning("[CIRCUIT BREAKER] HALF-OPEN: Initiating attempts to reconnect Redis...");
                            return true;
                        }
                    }
                }
                return false;
            }

            return true;
        }

        private void TripCircuit(Exception ex, string operation)
        {
            lock (_circuitLock)
            {
                if (_circuitState != CircuitState.Open)
                {
                    _circuitState = CircuitState.Open;
                    _nextRetryTime = DateTime.UtcNow.Add(CooldownTime);
                    _logger.LogCritical(ex, "[REDIS DOWN] Circuit TRIPPED at {Operation}! Ignore Redis entirely in the next {Seconds}s. Fallback to L1.", operation, CooldownTime.TotalSeconds);
                }
            }
        }

        private void ReportSuccess()
        {
            if (_circuitState == CircuitState.HalfOpen)
            {
                lock (_circuitLock)
                {
                    if (_circuitState == CircuitState.HalfOpen)
                    {
                        _circuitState = CircuitState.Closed;
                        _logger.LogInformation("[CIRCUIT BREAKER] CLOSED: Redis is now restored and functioning normally!");
                    }
                }
            }
        }

        public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
        {
            if (_memoryCache.TryGetValue(key, out T? l1Data))
            {
                return l1Data;
            }

            if (!IsRedisHealthy())
            {
                _logger.LogWarning("[CACHE MISS] Redis is crashing, L1 doesn't have key '{Key}'. This request will go straight to the Database!", key);
                return default;
            }

            try
            {
                var data = await _redisCache.GetStringAsync(key, cancellationToken);

                ReportSuccess();

                if (data == null) return default;

                var result = JsonSerializer.Deserialize<T>(data);

                _memoryCache.Set(key, result, TimeSpan.FromMinutes(1));
                return result;
            }
            catch (Exception ex)
            {
                TripCircuit(ex, $"GetAsync({key})");
                return default;
            }
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null, CancellationToken cancellationToken = default)
        {
            if (!IsRedisHealthy())
            {
                _logger.LogWarning("[SURVIVAL MODE] Temporarily save data to L1 (Memory) for key '{Key}' because Redis is down.", key);
                _memoryCache.Set(key, value, L1SurvivalDuration);
                return;
            }

            try
            {
                var options = new DistributedCacheEntryOptions();
                if (expiry.HasValue) options.SetAbsoluteExpiration(expiry.Value);

                var json = JsonSerializer.Serialize(value);
                await _redisCache.SetStringAsync(key, json, options, cancellationToken);

                ReportSuccess();

                _memoryCache.Set(key, value, expiry ?? L1SurvivalDuration);
            }
            catch (Exception ex)
            {
                TripCircuit(ex, $"SetAsync({key})");
                _memoryCache.Set(key, value, L1SurvivalDuration);
            }
        }

        public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
        {
            _memoryCache.Remove(key);

            if (!IsRedisHealthy()) return;

            try
            {
                await _redisCache.RemoveAsync(key, cancellationToken);
                ReportSuccess();
            }
            catch (Exception ex)
            {
                TripCircuit(ex, $"RemoveAsync({key})");
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
                ReportSuccess();
            }
            catch (Exception ex)
            {
                TripCircuit(ex, "RemoveMultipleAsync");
            }
        }
    }
}
