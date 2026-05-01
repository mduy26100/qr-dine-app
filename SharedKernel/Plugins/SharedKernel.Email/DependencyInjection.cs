namespace SharedKernel.Email
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddSharedBrevoEmail(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<EmailSettings>(configuration.GetSection(EmailSettings.SectionName));

            services.AddHttpClient<IEmailService, BrevoApiEmailService>();

            return services;
        }

        public static IServiceCollection AddSharedMailKitEmail(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<EmailSettings>(configuration.GetSection(EmailSettings.SectionName));

            services.AddTransient<IEmailService, MailKitEmailService>();

            return services;
        }
    }
}
