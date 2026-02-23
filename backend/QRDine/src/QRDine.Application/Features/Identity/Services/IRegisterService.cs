using QRDine.Application.Features.Identity.DTOs;

namespace QRDine.Application.Features.Identity.Services
{
    public interface IRegisterService
    {
        Task<RegisterResponseDto> RegisterMerchantAsync(RegisterRequestDto request, CancellationToken cancellationToken);
    }
}
