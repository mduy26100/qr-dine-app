namespace SharedKernel.ImageUpload.Services
{
    public class LocalImageUploadService : IImageUploadService
    {
        private readonly LocalImageSettings _settings;

        public LocalImageUploadService(IOptions<LocalImageSettings> options)
        {
            _settings = options.Value;
        }

        public async Task<string> UploadAsync(ImageUploadRequestDto request, CancellationToken cancellationToken = default)
        {
            if (!Directory.Exists(_settings.PhysicalPath))
            {
                Directory.CreateDirectory(_settings.PhysicalPath);
            }

            var extension = Path.GetExtension(request.FileName);
            var uniqueFileName = $"{Guid.NewGuid()}{extension}";

            var fullPath = Path.Combine(_settings.PhysicalPath, uniqueFileName);

            using (var fileStream = new FileStream(fullPath, FileMode.Create))
            {
                await request.Content.CopyToAsync(fileStream, cancellationToken);
            }

            var fileUrl = $"{_settings.BaseUrl.TrimEnd('/')}/{uniqueFileName}";

            return fileUrl;
        }
    }
}
