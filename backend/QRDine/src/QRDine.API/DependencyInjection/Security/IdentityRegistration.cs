using QRDine.Infrastructure.Identity.Models;
using QRDine.Infrastructure.Persistence;

namespace QRDine.API.DependencyInjection.Security
{
    public static class IdentityRegistration
    {
        public static IServiceCollection AddIdentityServices(
            this IServiceCollection services, IConfiguration configuration)
        {
            // ASP.NET Core Identity
            services.AddIdentity<ApplicationUser, ApplicationRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            // HTTP Context accessor for user claims
            services.AddHttpContextAccessor();

            return services;
        }
    }
}
