using QRDine.Application.Common.Models;
using QRDine.Application.Features.Staffs.DTOs;
using QRDine.Application.Features.Staffs.Queries.GetStaffsPaged;
using QRDine.Application.Features.Staffs.Services;
using QRDine.Infrastructure.Identity.Constants;
using QRDine.Infrastructure.Persistence;
using QRDine.Infrastructure.Staffs.Specifications;

namespace QRDine.Infrastructure.Staffs.Services
{
    public class StaffService : IStaffService
    {
        private readonly ApplicationDbContext _context;

        public StaffService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResult<StaffDto>> GetStaffsPagedAsync(Guid merchantId, GetStaffsPagedQuery request, CancellationToken cancellationToken)
        {
            var staffRoleId = await _context.Roles
                .Where(r => r.Name == SystemRoles.Staff)
                .Select(r => r.Id)
                .FirstOrDefaultAsync(cancellationToken);

            var baseQuery = _context.Users
                .AsNoTracking()
                .Where(u => _context.UserRoles.Any(ur => ur.UserId == u.Id && ur.RoleId == staffRoleId));

            var spec = new GetStaffsPagedSpec(merchantId, request.SearchTerm, request.PageNumber, request.PageSize);

            var totalCount = await SpecificationEvaluator.Default
                .GetQuery(baseQuery, spec, evaluateCriteriaOnly: true)
                .CountAsync(cancellationToken);

            var items = await SpecificationEvaluator.Default
                .GetQuery(baseQuery, spec)
                .ToListAsync(cancellationToken);

            return new PagedResult<StaffDto>(items, totalCount, request.PageNumber, request.PageSize);
        }
    }
}
