using QRDine.Application.Features.Staffs.DTOs;
using QRDine.Infrastructure.Identity.Models;
using QRDine.Infrastructure.Staffs.Extensions;

namespace QRDine.Infrastructure.Staffs.Specifications
{
    public class GetStaffsPagedSpec : Specification<ApplicationUser, StaffDto>
    {
        public GetStaffsPagedSpec(Guid merchantId, string? searchTerm, int pageNumber, int pageSize)
        {
            Query.Where(u => u.MerchantId == merchantId);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.ToLower();
                Query.Where(u =>
                    (u.FirstName != null && u.FirstName.ToLower().Contains(term)) ||
                    (u.LastName != null && u.LastName.ToLower().Contains(term)) ||
                    (u.Email != null && u.Email.ToLower().Contains(term)) ||
                    (u.PhoneNumber != null && u.PhoneNumber.Contains(term))
                );
            }

            Query.OrderByDescending(u => u.CreatedAt)
                 .Skip((pageNumber - 1) * pageSize)
                 .Take(pageSize);

            Query.Select(StaffExtensions.ToStaffDto);
        }
    }
}
