using SharedKernel.Application.Interfaces.FileUpload.Models;

namespace SharedKernel.Application.Interfaces.FileUpload
{
    public interface IFileUploadService
    {
        Task<string> UploadAsync(FileUploadRequestDto request, CancellationToken cancellationToken = default);
    }
}
