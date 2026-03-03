using QRDine.Application.Common.Abstractions.Identity;
using QRDine.Application.Features.Identity.Services;
using QRDine.Infrastructure.Identity.Models;
using QRDine.Infrastructure.Identity.Services;
using QRDine.Infrastructure.Persistence;

namespace QRDine.API.DependencyInjection.Security
{
    public static class IdentityRegistration
    {
        public static IServiceCollection AddIdentityServices(
            this IServiceCollection services, IConfiguration configuration)
        {
            // ASP.NET Core Identity
            services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
            {
                options.Password.RequiredLength = 6;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireDigit = true;
                options.Password.RequireNonAlphanumeric = true;

                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);

                options.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

            // JWT Token Generator
            services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

            // Current User Service
            services.AddScoped<ICurrentUserService, CurrentUserService>();

            // Features Identity Services
            services.AddScoped<ILoginService, LoginService>();
            services.AddScoped<IRegisterService, RegisterService>();
            services.AddScoped<ILogoutService, LogoutService>();
            services.AddScoped<IRefreshTokenService, RefreshTokenService>();

            return services;
        }
    }
}
