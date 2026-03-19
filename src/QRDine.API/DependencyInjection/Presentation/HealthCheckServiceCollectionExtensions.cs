namespace QRDine.API.DependencyInjection.Presentation
{
    public static class HealthCheckServiceCollectionExtensions
    {
        public static IServiceCollection AddAppHealthChecks(this IServiceCollection services, IConfiguration configuration)
        {
            var sqlConnectionString = configuration.GetConnectionString("DefaultConnection");

            var redisConnectionString = configuration["Redis:ConnectionString"];

            if (string.IsNullOrEmpty(redisConnectionString))
            {
                throw new ArgumentNullException(nameof(redisConnectionString), "Redis ConnectionString is missing in appsettings.json");
            }

            services.AddHealthChecks()
                .AddCheck("self", () => HealthCheckResult.Healthy(), tags: new[] { "live" })

                .AddSqlServer(
                    connectionString: sqlConnectionString!,
                    name: "sql-server",
                    failureStatus: HealthStatus.Unhealthy,
                    tags: new[] { "ready", "db", "sql" },
                    timeout: TimeSpan.FromSeconds(5)
                )

                .AddRedis(
                    redisConnectionString: redisConnectionString,
                    name: "redis-cache",
                    failureStatus: HealthStatus.Degraded,
                    tags: new[] { "ready", "cache", "redis" },
                    timeout: TimeSpan.FromSeconds(3)
                );

            return services;
        }
    }
}
