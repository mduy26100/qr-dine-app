using QRDine.Application.Common.Abstractions.PayOS;
using QRDine.Infrastructure.Configuration;
using QRDine.Infrastructure.PayOS;

namespace QRDine.API.DependencyInjection.Infrastructure
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddPayOS(this IServiceCollection services, IConfiguration configuration)
        {
            var settings = configuration
                .GetSection("PayOS")
                .Get<PayOsSettings>()
                ?? throw new Exception("PayOS configuration is missing");

            services.AddSingleton(new PayOSClient(
                settings.ClientId,
                settings.ApiKey,
                settings.ChecksumKey
            ));

            services.AddScoped<IPayOSService, PayOSService>();

            return services;
        }
    }
}
