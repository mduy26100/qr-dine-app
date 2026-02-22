namespace QRDine.Application.Features.Identity.DTOs
{
    public class LoginRequestDto
    {
        public required string Identifier { get; set; }
        public required string Password { get; set; }
    }
}
