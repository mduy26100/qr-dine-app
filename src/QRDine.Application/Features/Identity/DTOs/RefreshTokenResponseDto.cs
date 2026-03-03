namespace QRDine.Application.Features.Identity.DTOs
{
    public class RefreshTokenResponseDto
    {
        public string AccessToken { get; set; } = default!;

        [JsonIgnore]
        public string RefreshToken { get; set; } = default!;

        public int ExpiresInMinutes { get; set; }
    }
}
