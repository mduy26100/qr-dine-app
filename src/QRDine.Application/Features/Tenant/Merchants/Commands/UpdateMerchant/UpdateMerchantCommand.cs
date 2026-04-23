using QRDine.Application.Features.Tenant.Merchants.DTOs;

namespace QRDine.Application.Features.Tenant.Merchants.Commands.UpdateMerchant
{
    public record UpdateMerchantCommand(UpdateMerchantDto Dto) : IRequest<Unit>;
}
