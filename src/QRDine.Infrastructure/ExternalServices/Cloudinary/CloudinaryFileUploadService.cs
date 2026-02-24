using QRDine.Application.Common.Abstractions.ExternalServices;
using QRDine.Application.Common.Models;

namespace QRDine.Infrastructure.ExternalServices.Cloudinary
{
    public class CloudinaryFileUploadService : IFileUploadService
    {
        private readonly CloudinaryDotNet.Cloudinary _cloudinary;

        public CloudinaryFileUploadService(IOptions<CloudinarySettings> options)
        {
            var settings = options.Value;
            var account = new Account(settings.CloudName, settings.ApiKey, settings.ApiSecret);
            _cloudinary = new CloudinaryDotNet.Cloudinary(account);
        }

        public async Task<string> UploadAsync(FileUploadRequest request, CancellationToken cancellationToken = default)
        {
            var folderName = "QRDine";
            var fileNameWithoutExt = Path.GetFileNameWithoutExtension(request.FileName);

            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(request.FileName, request.Content),
                PublicId = fileNameWithoutExt,
                Folder = folderName,
                Overwrite = true,
                UseFilename = true,
                UniqueFilename = false
            };

            var result = await _cloudinary.UploadAsync(uploadParams, cancellationToken);

            if (result.StatusCode != System.Net.HttpStatusCode.OK)
                throw new Exception($"Cloudinary upload failed: {result.Error?.Message}");

            return result.SecureUrl.ToString();
        }
    }
}
