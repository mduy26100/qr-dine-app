using QRDine.Application.Features.Identity.DTOs;

namespace QRDine.Application.Features.Identity.Commands.Login
{
    public record LoginCommand(LoginRequestDto Dto) : IRequest<LoginResponseDto>;
}
