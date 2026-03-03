using QRDine.Application.Common.Abstractions.Cryptography;
using QRDine.Application.Features.Identity.DTOs;
using QRDine.Application.Features.Identity.Services;
using QRDine.Infrastructure.Identity.Constants;
using QRDine.Infrastructure.Identity.Models;
using QRDine.Infrastructure.Identity.Settings;
using QRDine.Infrastructure.Persistence;

namespace QRDine.Infrastructure.Identity.Services
{
    public class RefreshTokenService : IRefreshTokenService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ITokenSecurityService _tokenSecurityService;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly JwtSettings _jwtSettings;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public RefreshTokenService(
            ApplicationDbContext dbContext,
            ITokenSecurityService tokenSecurityService,
            IJwtTokenGenerator jwtTokenGenerator,
            UserManager<ApplicationUser> userManager,
            IOptions<JwtSettings> jwtOptions,
            IHttpContextAccessor httpContextAccessor)
        {
            _dbContext = dbContext;
            _tokenSecurityService = tokenSecurityService;
            _jwtTokenGenerator = jwtTokenGenerator;
            _userManager = userManager;
            _jwtSettings = jwtOptions.Value;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<RefreshTokenResponseDto> RefreshAsync(string plainRefreshToken, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(plainRefreshToken))
                throw new UnauthorizedAccessException("Refresh token is required.");

            var hashedIncomingToken = _tokenSecurityService.HashToken(plainRefreshToken);

            var tokenRecord = await _dbContext.RefreshTokens
                .IgnoreQueryFilters()
                .Include(rt => rt.User)
                .FirstOrDefaultAsync(rt => rt.Token == hashedIncomingToken, cancellationToken);

            if (tokenRecord == null)
                throw new UnauthorizedAccessException("Invalid refresh token.");

            if (tokenRecord.IsRevoked)
                throw new UnauthorizedAccessException("Refresh token has been revoked. Please login again.");

            if (tokenRecord.ExpiresAt <= DateTime.UtcNow)
                throw new UnauthorizedAccessException("Refresh token has expired. Please login again.");

            if (!_tokenSecurityService.VerifyToken(plainRefreshToken, tokenRecord.Token))
                throw new UnauthorizedAccessException("Token verification failed.");

            var user = tokenRecord.User;
            if (user == null)
                throw new UnauthorizedAccessException("User associated with token not found.");

            var currentIp = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();

            tokenRecord.IsRevoked = true;
            tokenRecord.RevokedAt = DateTime.UtcNow;
            tokenRecord.RevokedByIp = currentIp;

            var roles = await _userManager.GetRolesAsync(user);
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
                new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}".Trim())
            };

            if (user.MerchantId.HasValue)
                claims.Add(new Claim(AppClaimTypes.MerchantId, user.MerchantId.Value.ToString()));

            foreach (var role in roles)
                claims.Add(new Claim(ClaimTypes.Role, role));

            var newAccessToken = _jwtTokenGenerator.GenerateToken(claims);
            var newPlainRefreshToken = _jwtTokenGenerator.GenerateRefreshToken();
            var newHashedRefreshToken = _tokenSecurityService.HashToken(newPlainRefreshToken);

            var newRefreshTokenEntity = new RefreshToken
            {
                UserId = user.Id,
                Token = newHashedRefreshToken,
                ExpiresAt = _jwtTokenGenerator.GetRefreshTokenExpiration(),
                CreatedByIp = currentIp
            };

            _dbContext.RefreshTokens.Add(newRefreshTokenEntity);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return new RefreshTokenResponseDto
            {
                AccessToken = newAccessToken,
                RefreshToken = newPlainRefreshToken,
                ExpiresInMinutes = _jwtSettings.AccessTokenExpiryMinutes
            };
        }
    }
}
