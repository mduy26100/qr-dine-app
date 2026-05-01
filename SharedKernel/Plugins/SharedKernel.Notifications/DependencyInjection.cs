namespace SharedKernel.Notifications
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddSharedSignalRNotifications(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<SignalRSettings>(configuration.GetSection(SignalRSettings.SectionName));

            services.AddSignalR();
            services.AddTransient<IRealtimeNotifier, SignalRNotifierService>();

            return services;
        }

        public static IServiceCollection AddSignalRJwtAuthenticationInterceptor(this IServiceCollection services, IConfiguration configuration)
        {
            var settings = configuration.GetSection(SignalRSettings.SectionName).Get<SignalRSettings>() ?? new SignalRSettings();

            services.Configure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                var existingOnMessageReceived = options.Events?.OnMessageReceived;

                options.Events ??= new JwtBearerEvents();
                options.Events.OnMessageReceived = async context =>
                {
                    var accessToken = context.Request.Query["access_token"];
                    var path = context.HttpContext.Request.Path;

                    if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments(settings.HubRoute))
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

        public static void MapSharedNotificationHub(this WebApplication app)
        {
            var settings = app.Configuration.GetSection(SignalRSettings.SectionName).Get<SignalRSettings>() ?? new SignalRSettings();
            app.MapHub<GenericNotificationHub>(settings.HubRoute);
        }
    }
}
