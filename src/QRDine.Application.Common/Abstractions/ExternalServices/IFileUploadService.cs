using QRDine.Application.Common.Models;

namespace QRDine.Application.Common.Abstractions.ExternalServices
{
    public interface IFileUploadService
    {
        Task<string> UploadAsync(FileUploadRequest request, CancellationToken cancellationToken = default);
    }
}
