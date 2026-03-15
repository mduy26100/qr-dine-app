using QRDine.Application.Features.Staffs.DTOs;
using QRDine.Infrastructure.Identity.Models;

namespace QRDine.Infrastructure.Staffs.Extensions
{
    public static class StaffExtensions
    {
        public static Expression<Func<ApplicationUser, StaffDto>> ToStaffDto =>
            u => new StaffDto
            {
                Id = u.Id,
                FirstName = u.FirstName ?? string.Empty,
                LastName = u.LastName ?? string.Empty,
                Email = u.Email ?? string.Empty,
                PhoneNumber = u.PhoneNumber,
                IsActive = u.IsActive
            };
    }
}
