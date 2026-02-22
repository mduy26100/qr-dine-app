namespace QRDine.Infrastructure.Identity.Models
{
    public class RefreshToken
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid UserId { get; set; }
        public virtual ApplicationUser? User { get; set; }

        public string Token { get; set; } = default!;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime ExpiresAt { get; set; }
        public bool IsRevoked { get; set; } = false;

        [NotMapped]
        public bool IsActive => !IsRevoked && ExpiresAt > DateTime.UtcNow;

        public string? CreatedByIp { get; set; }
        public string? RevokedByIp { get; set; }
        public DateTime? RevokedAt { get; set; }
    }
}
