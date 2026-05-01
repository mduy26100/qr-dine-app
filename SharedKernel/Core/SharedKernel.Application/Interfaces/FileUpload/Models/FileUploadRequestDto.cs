namespace SharedKernel.Application.Interfaces.FileUpload.Models
{
    public class FileUploadRequestDto
    {
        public Stream Content { get; set; } = default!;
        public string FileName { get; set; } = default!;
    }
}
