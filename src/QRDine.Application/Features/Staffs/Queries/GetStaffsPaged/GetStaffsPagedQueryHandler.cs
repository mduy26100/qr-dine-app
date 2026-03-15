using QRDine.Application.Common.Abstractions.Identity;
using QRDine.Application.Common.Models;
using QRDine.Application.Features.Staffs.DTOs;
using QRDine.Application.Features.Staffs.Services;

namespace QRDine.Application.Features.Staffs.Queries.GetStaffsPaged
{
    public class GetStaffsPagedQueryHandler : IRequestHandler<GetStaffsPagedQuery, PagedResult<StaffDto>>
    {
        private readonly IStaffService _staffService;
        private readonly ICurrentUserService _currentUserService;

        public GetStaffsPagedQueryHandler(IStaffService staffService, ICurrentUserService currentUserService)
        {
            _staffService = staffService;
            _currentUserService = currentUserService;
        }

        public async Task<PagedResult<StaffDto>> Handle(GetStaffsPagedQuery request, CancellationToken cancellationToken)
        {
            var merchantId = _currentUserService.MerchantId;
            if (!merchantId.HasValue)
            {
                throw new UnauthorizedAccessException("Không xác định được Cửa hàng của bạn.");
            }

            return await _staffService.GetStaffsPagedAsync(merchantId.Value, request, cancellationToken);
        }
    }
}
