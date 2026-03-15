using QRDine.Application.Common.Abstractions.ExternalServices.FileUpload;
using QRDine.Application.Common.Abstractions.ExternalServices.QrCode;
using QRDine.Infrastructure.Configuration;
using QRDine.Infrastructure.ExternalServices.Cloudinary;
using QRDine.Infrastructure.ExternalServices.QrCode;

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

            // QrCode generation
            services.Configure<FrontendSettings>(
                configuration.GetSection("FrontendSettings"));

            services.AddScoped<IQrCodeService, QrCodeService>();

            return services;
        }
    }
}
