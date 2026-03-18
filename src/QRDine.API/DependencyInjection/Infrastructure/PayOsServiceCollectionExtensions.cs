using QRDine.Application.Common.Abstractions.PayOS;
using QRDine.Infrastructure.Configuration;
using QRDine.Infrastructure.PayOS;

namespace QRDine.API.DependencyInjection.Infrastructure
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddPayOS(this IServiceCollection services, IConfiguration configuration)
        {
            var settings = configuration.GetSection("PayOS").Get<PayOsSettings>()
                ?? throw new Exception("PayOS configuration is missing");

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
            services.AddScoped<IPayOSService, PayOSService>();

            return services;
        }
    }
}
