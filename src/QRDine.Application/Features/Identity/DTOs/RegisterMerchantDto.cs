namespace QRDine.Application.Features.Identity.DTOs
{
    public class RegisterMerchantDto
    {
        public string MerchantName { get; set; } = default!;
        public string? MerchantAddress { get; set; }
        public string? MerchantPhoneNumber { get; set; }

        public string FirstName { get; set; } = default!;
        public string LastName { get; set; } = default!;
        public string Email { get; set; } = default!;
        public string Password { get; set; } = default!;
        public string? UserPhoneNumber { get; set; }
    }
}
