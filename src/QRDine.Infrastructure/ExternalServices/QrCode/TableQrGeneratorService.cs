using Microsoft.AspNetCore.Hosting;
using QRDine.Application.Common.Abstractions.ExternalServices.FileUpload;
using QRDine.Application.Common.Abstractions.ExternalServices.QrCode;
using QRDine.Infrastructure.Configuration;

namespace QRDine.Infrastructure.ExternalServices.QrCode
{
    public class TableQrGeneratorService : ITableQrGeneratorService
    {
        private readonly IQrCodeService _qrCodeService;
        private readonly IFileUploadService _fileUploadService;
        private readonly FrontendSettings _settings;
        private readonly IWebHostEnvironment _env;

        public TableQrGeneratorService(
            IQrCodeService qrCodeService,
            IFileUploadService fileUploadService,
            IOptions<FrontendSettings> settings,
            IWebHostEnvironment env)
        {
            _qrCodeService = qrCodeService;
            _fileUploadService = fileUploadService;
            _settings = settings.Value;
            _env = env;
        }

        public async Task<string> GenerateAndUploadQrAsync(Guid merchantId, string tableToken, string tableName, CancellationToken cancellationToken = default)
        {
            var storefrontUrl = $"{_settings.BaseUrl.TrimEnd('/')}/storefront/{merchantId}/table/{tableToken}";

            var logoPath = Path.Combine(_env.WebRootPath, "images", "logo.png");

            var qrImageBytes = await _qrCodeService.GenerateQrCodeAsync(storefrontUrl, logoPath, cancellationToken);

            var fileName = $"qr_{merchantId}_{tableToken}.png";

            using var memoryStream = new MemoryStream(qrImageBytes);

            var uploadRequest = new FileUploadRequest
            {
                Content = memoryStream,
                FileName = fileName,
            };

            return await _fileUploadService.UploadAsync(uploadRequest, cancellationToken);
        }
    }
}
