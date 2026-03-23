using QRDine.Application.Common.Abstractions.Caching;
using QRDine.Infrastructure.Caching;

namespace QRDine.API.DependencyInjection.Infrastructure
{
    public static class CachingServiceCollectionExtensions
    {
        public static IServiceCollection AddCaching(
            this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetValue<string>("Redis:ConnectionString");

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException(
                    "Redis:ConnectionString is missing. Please check your appsettings.json or environment variables.");
            }

            var redisOptions = ConfigurationOptions.Parse(connectionString);

            redisOptions.ConnectTimeout = 150;
            redisOptions.SyncTimeout = 150;
            redisOptions.AsyncTimeout = 150;

            redisOptions.AbortOnConnectFail = false;

            services.AddStackExchangeRedisCache(options =>
            {
                options.ConfigurationOptions = redisOptions;
            });

            services.AddSingleton<ICacheService, RedisCacheService>();
            services.AddMemoryCache();

            return services;
        }
    }
}
