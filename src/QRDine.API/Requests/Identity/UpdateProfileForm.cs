using QRDine.Application.Features.Identity.DTOs;

namespace QRDine.API.Requests.Identity
{
    public class UpdateProfileForm
    {
        public string FirstName { get; set; } = default!;
        public string LastName { get; set; } = default!;
        public string? PhoneNumber { get; set; }
        public IFormFile? ImageFile { get; set; }

        public UpdateProfileRequestDto ToDto()
        {
            return new UpdateProfileRequestDto
            {
                FirstName = FirstName.Trim(),
                LastName = LastName.Trim(),
                PhoneNumber = PhoneNumber?.Trim(),
                ImgContent = ImageFile?.OpenReadStream(),
                ImgFileName = ImageFile?.FileName
            };
        }
    }
}
