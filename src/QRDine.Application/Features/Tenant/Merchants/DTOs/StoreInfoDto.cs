namespace QRDine.Application.Features.Tenant.Merchants.DTOs
{
    public class StoreInfoDto
    {
        public Guid MerchantId { get; set; }
        public string Name { get; set; } = default!;
        public string Slug { get; set; } = default!;
        public string? Address { get; set; }
        public string? PhoneNumber { get; set; }
        public string? LogoUrl { get; set; }
    }
}
