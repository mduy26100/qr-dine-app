using SharedKernel.Application.Interfaces.ImageUpload.Models;

namespace SharedKernel.Application.Interfaces.ImageUpload
{
    public interface IImageUploadService
    {
        Task<string> UploadAsync(ImageUploadRequestDto request, CancellationToken cancellationToken = default);
    }
}
