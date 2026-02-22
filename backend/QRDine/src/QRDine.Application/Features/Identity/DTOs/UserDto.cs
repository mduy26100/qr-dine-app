namespace QRDine.Application.Features.Identity.DTOs
{
    public class UserDto
    {
        public Guid Id { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string FirstName { get; set; } = default!;
        public string LastName { get; set; } = default!;
        public string? AvatarUrl { get; set; }

        public IList<string> Roles { get; set; } = new List<string>();
        public Guid? MerchantId { get; set; }
    }
}
