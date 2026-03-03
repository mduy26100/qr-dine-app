using QRDine.Application.Common.Abstractions.Cryptography;
using QRDine.Infrastructure.Configuration;
using QRDine.Infrastructure.Cryptography;

namespace QRDine.API.DependencyInjection.Infrastructure
{
    public static class CryptographyRegistration
    {
        public static IServiceCollection AddCryptography(
            this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<SecuritySettings>(configuration.GetSection("SecuritySettings"));
            services.AddSingleton<ITokenSecurityService, TokenSecurityService>();

            return services;
        }
    }
}
