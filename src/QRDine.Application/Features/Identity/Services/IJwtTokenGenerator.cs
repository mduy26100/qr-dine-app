namespace QRDine.Application.Features.Identity.Services
{
    public interface IJwtTokenGenerator
    {
        string GenerateToken(IEnumerable<Claim> claims);
        string GenerateRefreshToken();
        DateTime GetRefreshTokenExpiration();
        ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
    }
}
