using QRDine.Application.Features.Identity.DTOs;

namespace QRDine.Application.Features.Identity.Services
{
    public interface ILoginService
    {
        Task<LoginResponseDto> AuthenticateAsync(LoginRequestDto dto, CancellationToken cancellationToken);
    }
}
