using QRDine.Application.Common.Models;
using QRDine.Application.Features.Staffs.DTOs;

namespace QRDine.Application.Features.Staffs.Queries.GetStaffsPaged
{
    public class GetStaffsPagedQuery : PaginationRequest, IRequest<PagedResult<StaffDto>>
    {
        public string? SearchTerm { get; set; }
    }
}
