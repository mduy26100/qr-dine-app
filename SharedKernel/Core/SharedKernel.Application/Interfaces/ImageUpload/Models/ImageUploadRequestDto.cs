namespace SharedKernel.Application.Interfaces.ImageUpload.Models
{
    public class ImageUploadRequestDto
    {
        public Stream Content { get; set; } = default!;
        public string FileName { get; set; } = default!;
    }
}
