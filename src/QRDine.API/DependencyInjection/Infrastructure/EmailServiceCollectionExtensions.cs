using QRDine.Application.Common.Abstractions.Email;
using QRDine.Infrastructure.Configuration;
using QRDine.Infrastructure.Email;

namespace QRDine.API.DependencyInjection.Infrastructure
{
    public static class EmailServiceCollectionExtensions
    {
        public static IServiceCollection AddEmailService(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));
            services.AddTransient<IEmailService, MailKitEmailService>();

            return services;
        }
    }
}
