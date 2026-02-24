namespace QRDine.Application.Features.Identity.DTOs
{
    public class LoginResponseDto
    {
        public string AccessToken { get; set; } = default!;
        public string RefreshToken { get; set; } = default!;
        public int ExpiresInMinutes { get; set; }

        public UserDto User { get; set; } = default!;
    }
}
