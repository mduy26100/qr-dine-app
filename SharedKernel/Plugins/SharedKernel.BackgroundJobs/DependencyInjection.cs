namespace SharedKernel.BackgroundJobs
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddSharedHangfireBackgroundJobs(this IServiceCollection services, 
            Action<IServiceProvider, IGlobalConfiguration> configureStorage)
        {
            if (configureStorage == null)
            {
                throw new ArgumentNullException(nameof(configureStorage), "You need to configure a Storage Provider for Hangfire (e.g., SQL Server, Redis...).");
            }

            services.AddHangfire((sp, config) =>
            {
                config.SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                      .UseSimpleAssemblyNameTypeSerializer()
                      .UseRecommendedSerializerSettings();

                configureStorage.Invoke(sp, config);
            });

            services.AddHangfireServer();

            services.AddTransient<IBackgroundJobService, HangfireBackgroundJobService>();

            return services;
        }
    }
}
