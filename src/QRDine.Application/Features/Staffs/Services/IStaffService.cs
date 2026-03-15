using QRDine.Application.Common.Models;
using QRDine.Application.Features.Staffs.DTOs;
using QRDine.Application.Features.Staffs.Queries.GetStaffsPaged;

namespace QRDine.Application.Features.Staffs.Services
{
    public interface IStaffService
    {
        Task<PagedResult<StaffDto>> GetStaffsPagedAsync(Guid merchantId, GetStaffsPagedQuery request, CancellationToken cancellationToken);
    }
}
