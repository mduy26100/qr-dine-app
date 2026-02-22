namespace QRDine.Infrastructure.Identity.Models
{
    public class ApplicationUser : IdentityUser<Guid>
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? AvatarUrl { get; set; }

        [NotMapped]
        public string FullName => $"{FirstName} {LastName}".Trim();

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Guid? MerchantId { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
