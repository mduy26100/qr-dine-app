using QRDine.Application.Features.Identity.Services;
using QRDine.Infrastructure.Persistence;

namespace QRDine.Infrastructure.Identity.Services
{
    public class LogoutService : ILogoutService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public LogoutService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task LogoutAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var tokenRecords = await _context.RefreshTokens
                .IgnoreQueryFilters()
                .Where(t => t.UserId == userId && !t.IsRevoked && t.ExpiresAt > DateTime.UtcNow)
                .ToListAsync(cancellationToken);
             
            if (!tokenRecords.Any()) return;

            var ipAddress = _httpContextAccessor.HttpContext?
                .Connection.RemoteIpAddress?
                .ToString();

            foreach (var token in tokenRecords)
            {
                token.IsRevoked = true;
                token.RevokedAt = DateTime.UtcNow;
                token.RevokedByIp = string.IsNullOrWhiteSpace(ipAddress) ? "Unknown" : ipAddress;
            }

            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
