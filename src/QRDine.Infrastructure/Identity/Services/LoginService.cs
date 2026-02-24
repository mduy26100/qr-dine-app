using QRDine.Application.Features.Identity.DTOs;
using QRDine.Application.Features.Identity.Services;
using QRDine.Infrastructure.Identity.Constants;
using QRDine.Infrastructure.Identity.Models;
using QRDine.Infrastructure.Identity.Settings;
using QRDine.Infrastructure.Persistence;

namespace QRDine.Infrastructure.Identity.Services
{
    public class LoginService : ILoginService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;
        private readonly ApplicationDbContext _dbContext;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly JwtSettings _jwtSettings;

        public LoginService(
            UserManager<ApplicationUser> userManager,
            IJwtTokenGenerator jwtTokenGenerator,
            ApplicationDbContext dbContext,
            IHttpContextAccessor httpContextAccessor,
            IOptions<JwtSettings> jwtOptions)
        {
            _userManager = userManager;
            _jwtTokenGenerator = jwtTokenGenerator;
            _dbContext = dbContext;
            _httpContextAccessor = httpContextAccessor;
            _jwtSettings = jwtOptions.Value;
        }

        public async Task<LoginResponseDto> AuthenticateAsync(LoginRequestDto dto, CancellationToken cancellationToken)
        {
            ApplicationUser? user;

            if (dto.Identifier.Contains('@'))
            {
                user = await _userManager.FindByEmailAsync(dto.Identifier);
            }
            else
            {
                user = await _dbContext.Users
                    .FirstOrDefaultAsync(u => u.PhoneNumber == dto.Identifier, cancellationToken);
            }

            if (user == null)
            {
                throw new UnauthorizedAccessException("Invalid login credentials.");
            }

            var isPasswordValid = await _userManager.CheckPasswordAsync(user, dto.Password);
            if (!isPasswordValid)
            {
                throw new UnauthorizedAccessException("Invalid login credentials.");
            }

            var roles = await _userManager.GetRolesAsync(user);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
                new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}".Trim())
            };

            if (user.MerchantId.HasValue)
            {
                claims.Add(new Claim(AppClaimTypes.MerchantId, user.MerchantId.Value.ToString()));
            }

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var accessToken = _jwtTokenGenerator.GenerateToken(claims);
            var refreshTokenString = _jwtTokenGenerator.GenerateRefreshToken();

            var refreshTokenEntity = new RefreshToken
            {
                UserId = user.Id,
                Token = refreshTokenString,
                ExpiresAt = _jwtTokenGenerator.GetRefreshTokenExpiration(),
                CreatedByIp = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString()
            };

            _dbContext.Set<RefreshToken>().Add(refreshTokenEntity);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return new LoginResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshTokenString,

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
                    MerchantId = user.MerchantId
                }
            };
        }
    }
}