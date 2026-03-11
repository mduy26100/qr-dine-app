using QRDine.Application.Common.Abstractions.Notifications;
using QRDine.Infrastructure.SignalR.Services;

namespace QRDine.API.DependencyInjection.Infrastructure
{
    public static class SignalRServiceCollectionExtensions
    {
        public static IServiceCollection AddSignalRInfrastructure(this IServiceCollection services)
        {
            services.AddSignalR();

            services.AddScoped<IOrderNotificationService, OrderNotificationService>();

            services.Configure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                var existingOnMessageReceived = options.Events?.OnMessageReceived;

                options.Events ??= new JwtBearerEvents();
                options.Events.OnMessageReceived = async context =>
                {
                    var accessToken = context.Request.Query["access_token"];
                    var path = context.HttpContext.Request.Path;

                    if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs/order"))
                    {
                        context.Token = accessToken;
                    }
                    if (existingOnMessageReceived != null)
                    {
                        await existingOnMessageReceived(context);
                    }
                };
            });

            return services;
        }
    }
}
