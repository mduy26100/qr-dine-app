namespace SharedKernel.Payment
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddSharedPayOSPayment(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<PayOSSettings>(configuration.GetSection(PayOSSettings.SectionName));

            var settings = configuration.GetSection(PayOSSettings.SectionName).Get<PayOSSettings>();

            if (settings == null || string.IsNullOrWhiteSpace(settings.ClientId))
            {
                throw new InvalidOperationException("PayOS configuration is missing in appsettings.json under 'PayOSSettings'.");
            }

            var httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(30)
            };

            var payOSClient = new PayOSClient(new PayOSOptions
            {
                ClientId = settings.ClientId,
                ApiKey = settings.ApiKey,
                ChecksumKey = settings.ChecksumKey,
                HttpClient = httpClient
            });

            services.AddSingleton(payOSClient);
            services.AddScoped<IPaymentService, PayOSPaymentService>();

            return services;
        }
    }
}
