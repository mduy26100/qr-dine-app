namespace QRDine.Application.Common.Abstractions.ExternalServices.FileUpload
{
    public interface IFileUploadService
    {
        Task<string> UploadAsync(FileUploadRequest request, CancellationToken cancellationToken = default);
    }
}
