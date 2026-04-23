using QRDine.Application.Features.Tenant.Merchants.DTOs;

namespace QRDine.API.Requests.Tenant
{
    public class UpdateMerchantForm
    {
        public string Name { get; set; } = default!;
        public string Slug { get; set; } = default!;
        public string? Address { get; set; }
        public string? PhoneNumber { get; set; }
        public bool IsActive { get; set; } = true;
        public IFormFile? ImageFile { get; set; }

        public UpdateMerchantDto ToDto()
        {
            return new UpdateMerchantDto
            {
                Name = Name.Trim(),
                Slug = Slug.Trim(),
                Address = Address?.Trim(),
                PhoneNumber = PhoneNumber?.Trim(),
                IsActive = IsActive,
                ImgContent = ImageFile?.OpenReadStream(),
                ImgFileName = ImageFile?.FileName
            };
        }
    }
}
