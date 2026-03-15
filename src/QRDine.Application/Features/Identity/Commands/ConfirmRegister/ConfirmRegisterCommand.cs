using QRDine.Application.Features.Identity.DTOs;

namespace QRDine.Application.Features.Identity.Commands.ConfirmRegister
{
    public record ConfirmRegisterCommand(string Token) : IRequest<RegisterResponseDto>;
}
