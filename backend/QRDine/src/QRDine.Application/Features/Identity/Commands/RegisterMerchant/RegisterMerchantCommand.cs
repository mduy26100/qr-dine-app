using QRDine.Application.Features.Identity.DTOs;

namespace QRDine.Application.Features.Identity.Commands.RegisterMerchant
{
    public record RegisterMerchantCommand(RegisterMerchantDto Dto) : IRequest<RegisterResponseDto>;
}
