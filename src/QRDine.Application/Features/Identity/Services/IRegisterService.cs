using QRDine.Application.Features.Identity.DTOs;

namespace QRDine.Application.Features.Identity.Services
{
    public interface IRegisterService
    {
        Task ValidateNewMerchantAsync(RegisterMerchantDto request, CancellationToken cancellationToken);
        string GenerateActivationLink(string token);
        Task<RegisterResponseDto> ConfirmMerchantRegistrationAsync(RegisterMerchantDto request, CancellationToken cancellationToken);
        Task<RegisterResponseDto> RegisterStaffAsync(RegisterStaffDto request, Guid merchantId, CancellationToken cancellationToken);
    }
}
