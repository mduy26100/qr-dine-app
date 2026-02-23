using QRDine.Application.Features.Identity.DTOs;

namespace QRDine.Application.Features.Identity.Commands.RegisterStaff
{
    public record RegisterStaffCommand(RegisterStaffDto Dto) : IRequest<RegisterResponseDto>;
}
