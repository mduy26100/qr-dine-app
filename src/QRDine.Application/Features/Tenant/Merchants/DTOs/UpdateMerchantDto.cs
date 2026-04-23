namespace QRDine.Application.Features.Tenant.Merchants.DTOs
{
    public class UpdateMerchantDto
    {
        public string Name { get; set; } = default!;
        public string Slug { get; set; } = default!;
        public string? Address { get; set; }
        public string? PhoneNumber { get; set; }
        public bool IsActive { get; set; } = true;
        public Stream? ImgContent { get; set; }
        public string? ImgFileName { get; set; }
    }
}
