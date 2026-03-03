using QRDine.Application.Features.Identity.DTOs;
using QRDine.Application.Features.Identity.Services;

namespace QRDine.Application.Features.Identity.Commands.RefreshToken
{
    public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, RefreshTokenResponseDto>
    {
        private readonly IRefreshTokenService _refreshTokenService;

        public RefreshTokenCommandHandler(IRefreshTokenService refreshTokenService)
        {
            _refreshTokenService = refreshTokenService;
        }

        public async Task<RefreshTokenResponseDto> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
        {
            return await _refreshTokenService.RefreshAsync(request.refreshToken, cancellationToken);
        }
    }
}
