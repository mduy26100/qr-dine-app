using QRDine.Application.Common.Abstractions.ExternalServices.FileUpload;
using QRDine.Application.Common.Abstractions.ExternalServices.QrCode;
using QRDine.Infrastructure.Configuration;

namespace QRDine.Infrastructure.ExternalServices.QrCode
{
    public class TableQrGeneratorService : ITableQrGeneratorService
    {
        private readonly IQrCodeService _qrCodeService;
        private readonly IFileUploadService _fileUploadService;
        private readonly StorefrontSettings _settings;

        public TableQrGeneratorService(
            IQrCodeService qrCodeService,
            IFileUploadService fileUploadService,
            IOptions<StorefrontSettings> settings)
        {
            _qrCodeService = qrCodeService;
            _fileUploadService = fileUploadService;
            _settings = settings.Value;
        }

        public async Task<string> GenerateAndUploadQrAsync(Guid merchantId, string tableToken, string tableName, CancellationToken cancellationToken = default)
        {
            var storefrontUrl = $"{_settings.BaseUrl.TrimEnd('/')}/{merchantId}/table/{tableToken}";

            var qrImageBytes = await _qrCodeService.GenerateQrCodeAsync(storefrontUrl, cancellationToken);

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
