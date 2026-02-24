using QRDine.Application.Common.Abstractions.ExternalServices.FileUpload;
using QRDine.Infrastructure.ExternalServices.Cloudinary;

namespace QRDine.API.DependencyInjection.Infrastructure
{
    public static class ExternalServicesRegistration
    {
        public static IServiceCollection AddExternalServices(
            this IServiceCollection services, IConfiguration configuration)
        {
            // Cloudinary file upload
            services.Configure<CloudinarySettings>(
                configuration.GetSection("Cloudinary"));

            services.AddScoped<IFileUploadService, CloudinaryFileUploadService>();

            return services;
        }
    }
}
