using QRDine.Application.Common.Abstractions.Caching;
using QRDine.Infrastructure.Caching;

namespace QRDine.API.DependencyInjection.Infrastructure
{
    public static class CachingServiceCollectionExtensions
    {
        public static IServiceCollection AddCaching(
            this IServiceCollection services, IConfiguration configuration)
        {
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = configuration.GetValue<string>("Redis:ConnectionString");
            });

            services.AddScoped<ICacheService, RedisCacheService>();

            return services;
        }
    }
}
