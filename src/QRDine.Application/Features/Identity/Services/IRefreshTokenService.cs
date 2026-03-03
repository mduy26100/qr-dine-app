using QRDine.Application.Features.Identity.DTOs;

namespace QRDine.Application.Features.Identity.Services
{
    public interface IRefreshTokenService
    {
        Task<RefreshTokenResponseDto> RefreshAsync(string plainRefreshToken, CancellationToken cancellationToken = default);
    }
}
