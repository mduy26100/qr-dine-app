using QRDine.Application.Common.Abstractions.Cryptography;
using QRDine.Application.Features.Billing.Subscriptions.Services;
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
        private readonly ISubscriptionService _subscriptionService;

        public RefreshTokenService(
            ApplicationDbContext dbContext,
            ITokenSecurityService tokenSecurityService,
            IJwtTokenGenerator jwtTokenGenerator,
            UserManager<ApplicationUser> userManager,
            IOptions<JwtSettings> jwtOptions,
            IHttpContextAccessor httpContextAccessor,
            ISubscriptionService subscriptionService)
        {
            _dbContext = dbContext;
            _tokenSecurityService = tokenSecurityService;
            _jwtTokenGenerator = jwtTokenGenerator;
            _userManager = userManager;
            _jwtSettings = jwtOptions.Value;
            _httpContextAccessor = httpContextAccessor;
            _subscriptionService = subscriptionService;
        }

        public async Task<RefreshTokenResponseDto> RefreshAsync(string plainRefreshToken, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(plainRefreshToken))
                throw new UnauthorizedAccessException("Invalid or expired refresh token.");

            var hashedIncomingToken = _tokenSecurityService.HashToken(plainRefreshToken);

            var tokenRecord = await _dbContext.RefreshTokens
                .IgnoreQueryFilters()
                .Include(rt => rt.User)
                .FirstOrDefaultAsync(rt => rt.Token == hashedIncomingToken, cancellationToken);

            if (tokenRecord == null)
            {
                throw new UnauthorizedAccessException("Invalid or expired refresh token.");
            }

            if (tokenRecord.IsRevoked)
            {
                var activeTokens = await _dbContext.RefreshTokens
                    .IgnoreQueryFilters()
                    .Where(t => t.UserId == tokenRecord.UserId && !t.IsRevoked)
                    .ToListAsync(cancellationToken);

                foreach (var token in activeTokens)
                {
                    token.IsRevoked = true;
                    token.RevokedAt = DateTime.UtcNow;
                    token.RevokedByIp = "System: Token Reuse Detected";
                }

                await _dbContext.SaveChangesAsync(cancellationToken);
                throw new UnauthorizedAccessException("Invalid or expired refresh token.");
            }

            if (tokenRecord.ExpiresAt <= DateTime.UtcNow || tokenRecord.User == null)
            {
                throw new UnauthorizedAccessException("Invalid or expired refresh token.");
            }

            var user = tokenRecord.User;
            var ipAddress = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();
            var currentIp = string.IsNullOrWhiteSpace(ipAddress) ? "Unknown" : ipAddress;

            tokenRecord.IsRevoked = true;
            tokenRecord.RevokedAt = DateTime.UtcNow;
            tokenRecord.RevokedByIp = currentIp;

            var roles = await _userManager.GetRolesAsync(user);
            string? currentPlanCode = null;
            string? currentSubStatus = null;

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
                new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}".Trim())
            };

            if (user.MerchantId.HasValue)
            {
                claims.Add(new Claim(AppClaimTypes.MerchantId, user.MerchantId.Value.ToString()));

                var subInfo = await _subscriptionService.GetLatestSubscriptionInfoAsync(user.MerchantId.Value, cancellationToken);
                if (subInfo != null)
                {
                    currentPlanCode = subInfo.PlanCode;
                    var isExpired = subInfo.EndDate < DateTime.UtcNow;
                    currentSubStatus = isExpired ? "Expired" : subInfo.Status.ToString();

                    claims.Add(new Claim(AppClaimTypes.PlanCode, currentPlanCode));
                    claims.Add(new Claim(AppClaimTypes.SubscriptionStatus, currentSubStatus));
                }
            }

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

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
                ExpiresInMinutes = _jwtSettings.AccessTokenExpiryMinutes,
                User = new UserDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    FirstName = user.FirstName ?? string.Empty,
                    LastName = user.LastName ?? string.Empty,
                    AvatarUrl = user.AvatarUrl,
                    Roles = roles,
                    MerchantId = user.MerchantId,
                    PlanCode = currentPlanCode,
                    SubscriptionStatus = currentSubStatus
                }
            };
        }
    }
}
