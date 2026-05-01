namespace SharedKernel.ImageUpload.Services
{
    public class CloudinaryImageUploadService : IImageUploadService
    {
        private readonly Cloudinary _cloudinary;
        private readonly CloudinarySettings _settings;

        public CloudinaryImageUploadService(IOptions<CloudinarySettings> options)
        {
            _settings = options.Value;
            var account = new Account(_settings.CloudName, _settings.ApiKey, _settings.ApiSecret);
            _cloudinary = new Cloudinary(account);
        }

        public async Task<string> UploadAsync(ImageUploadRequestDto request, CancellationToken cancellationToken = default)
        {
            var fileNameWithoutExt = Path.GetFileNameWithoutExtension(request.FileName);
            var uniqueFileName = $"{fileNameWithoutExt}_{Guid.NewGuid():N}";

            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(request.FileName, request.Content),
                PublicId = uniqueFileName,
                Folder = _settings.FolderName,
                Overwrite = true,
                UseFilename = true,
                UniqueFilename = false
            };

            var result = await _cloudinary.UploadAsync(uploadParams, cancellationToken);

            if (result.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception($"Cloudinary upload failed: {result.Error?.Message}");
            }

            return result.SecureUrl.ToString();
        }
    }
}
