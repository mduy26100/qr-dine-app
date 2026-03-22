using QRDine.Application.Features.Identity.DTOs;

namespace QRDine.Application.Features.Identity.Commands.ChangePassword
{
    public record ChangePasswordCommand(ChangePasswordRequestDto Dto) : IRequest<Unit>;
}
