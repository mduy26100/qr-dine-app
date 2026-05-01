namespace SharedKernel.Caching
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddSharedRedisCaching(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<RedisSettings>(configuration.GetSection(RedisSettings.SectionName));

            var settings = configuration.GetSection(RedisSettings.SectionName).Get<RedisSettings>();

            if (settings == null || string.IsNullOrWhiteSpace(settings.ConnectionString))
            {
                throw new InvalidOperationException("Redis ConnectionString is missing in appsettings.json under 'RedisSettings'.");
            }

            services.AddStackExchangeRedisCache(options =>
            {
                var redisOptions = ConfigurationOptions.Parse(settings.ConnectionString);
                redisOptions.ConnectTimeout = settings.ConnectTimeoutMs;
                redisOptions.SyncTimeout = settings.SyncTimeoutMs;
                redisOptions.AsyncTimeout = settings.AsyncTimeoutMs;
                redisOptions.AbortOnConnectFail = settings.AbortOnConnectFail;

                options.ConfigurationOptions = redisOptions;
            });

            services.AddMemoryCache();
            services.AddSingleton<ICacheService, RedisCircuitBreakerCacheService>();

            return services;
        }

        public static IServiceCollection AddSharedMemoryOnlyCaching(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<MemoryCacheSettings>(configuration.GetSection(MemoryCacheSettings.SectionName));

            var settings = configuration.GetSection(MemoryCacheSettings.SectionName).Get<MemoryCacheSettings>();

            services.AddMemoryCache(options =>
            {
                if (settings != null)
                {
                    options.SizeLimit = settings.SizeLimit;
                    options.ExpirationScanFrequency = TimeSpan.FromSeconds(settings.ExpirationScanFrequencySeconds);
                }
            });

            services.AddSingleton<ICacheService, MemoryOnlyCacheService>();

            return services;
        }
    }
}
