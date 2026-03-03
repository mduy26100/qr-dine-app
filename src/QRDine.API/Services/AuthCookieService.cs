using QRDine.API.Constants;
using QRDine.Infrastructure.Identity.Settings;

namespace QRDine.API.Services
{
    public class AuthCookieService : IAuthCookieService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly JwtSettings _jwtSettings;

        public AuthCookieService(IHttpContextAccessor httpContextAccessor, IOptions<JwtSettings> jwtOptions)
        {
            _httpContextAccessor = httpContextAccessor;
            _jwtSettings = jwtOptions.Value;
        }

        public void AppendRefreshTokenCookie(string refreshToken)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpiryDays),
                Path = "/"
            };

            _httpContextAccessor.HttpContext?.Response.Cookies.Append(
                CookieNames.RefreshToken,
                refreshToken,
                cookieOptions);
        }

        public void DeleteRefreshTokenCookie()
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTime.UtcNow.AddDays(-1),
                Path = "/"
            };

            _httpContextAccessor.HttpContext?.Response.Cookies.Delete(
                CookieNames.RefreshToken,
                cookieOptions);
        }
    }
}
