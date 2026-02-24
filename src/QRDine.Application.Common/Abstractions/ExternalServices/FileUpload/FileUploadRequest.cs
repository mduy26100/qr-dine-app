namespace QRDine.Application.Common.Abstractions.ExternalServices.FileUpload
{
    public class FileUploadRequest
    {
        public Stream Content { get; set; } = default!;
        public string FileName { get; set; } = default!;
        public string ContentType { get; set; } = default!;
    }
}
