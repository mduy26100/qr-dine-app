namespace QRDine.Application.Features.Identity.DTOs
{
    public class UpdateProfileRequestDto
    {
        public string FirstName { get; set; } = default!;
        public string LastName { get; set; } = default!;
        public string? PhoneNumber { get; set; }
        public Stream? ImgContent { get; set; }
        public string? ImgFileName { get; set; }
    }
}
