using QRDine.Application.Features.Tenant.Merchants.DTOs;
using QRDine.Application.Features.Tenant.Merchants.Extensions;
using QRDine.Domain.Tenant;

namespace QRDine.Application.Features.Tenant.Merchants.Specifications
{
    public class MerchantProfileSpec : Specification<Merchant, MerchantProfileProjectionDto>
    {
        public MerchantProfileSpec(Guid merchantId)
        {
            Query.Where(m => m.Id == merchantId && m.IsActive);

            Query.Select(MerchantProfileExtensions.ToProfileProjection);
        }
    }
}
