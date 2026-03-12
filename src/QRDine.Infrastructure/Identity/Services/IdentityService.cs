using QRDine.Application.Common.Abstractions.Identity;
using QRDine.Infrastructure.Identity.Constants;
using QRDine.Infrastructure.Persistence;

namespace QRDine.Infrastructure.Identity.Services
{
    public class IdentityService : IIdentityService
    {
        private readonly ApplicationDbContext _context;

        public IdentityService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<int> CountStaffByMerchantAsync(Guid merchantId, CancellationToken cancellationToken)
        {
            return await _context.Users
                .Where(u => u.MerchantId == merchantId && u.IsActive)
                .Join(_context.UserRoles,
                      user => user.Id,
                      ur => ur.UserId,
                      (user, ur) => ur)
                .Join(_context.Roles,
                      ur => ur.RoleId,
                      role => role.Id,
                      (ur, role) => role)
                .Where(role => role.Name == SystemRoles.Staff)
                .CountAsync(cancellationToken);
        }
    }
}
