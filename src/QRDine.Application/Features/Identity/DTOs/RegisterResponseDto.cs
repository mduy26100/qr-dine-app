namespace QRDine.Application.Features.Identity.DTOs
{
    public class RegisterResponseDto
    {
        public Guid MerchantId { get; set; }
        public string MerchantName { get; set; } = default!;
        public string FirstName { get; set; } = default!;
        public string LastName { get; set; } = default!;
    }
}
