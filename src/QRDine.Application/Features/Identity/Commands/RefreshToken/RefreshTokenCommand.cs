using QRDine.Application.Features.Identity.DTOs;

namespace QRDine.Application.Features.Identity.Commands.RefreshToken
{
    public record RefreshTokenCommand(string refreshToken) : IRequest<RefreshTokenResponseDto>;
}
