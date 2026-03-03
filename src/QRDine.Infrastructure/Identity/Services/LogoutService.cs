using QRDine.Application.Features.Identity.Services;
using QRDine.Infrastructure.Persistence;

namespace QRDine.Infrastructure.Identity.Services
{
    public class LogoutService : ILogoutService
    {
        private readonly ApplicationDbContext _context;

        public LogoutService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task LogoutAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var tokenRecords = await _context.RefreshTokens
                .IgnoreQueryFilters()
                .Where(t => t.UserId == userId && !t.IsRevoked && t.ExpiresAt > DateTime.UtcNow)
                .ToListAsync(cancellationToken);
             
            if (!tokenRecords.Any()) return;

            foreach (var token in tokenRecords)
            {
                token.IsRevoked = true;
                token.RevokedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
