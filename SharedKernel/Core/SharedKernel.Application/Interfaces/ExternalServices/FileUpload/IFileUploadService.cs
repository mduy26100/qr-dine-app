namespace SharedKernel.Application.Interfaces.ExternalServices.FileUpload
{
    public interface IFileUploadService
    {
        Task<string> UploadAsync(FileUploadRequest request, CancellationToken cancellationToken = default);
    }
}
