namespace QRDine.Infrastructure.Identity.Settings
{
    public class JwtSettings
    {
        public string Secret { get; set; } = null!;
        public string ValidIssuer { get; set; } = null!;
        public string ValidAudience { get; set; } = null!;
        public int AccessTokenExpiryMinutes { get; set; }
        public int RefreshTokenExpiryDays { get; set; }
    }
}
