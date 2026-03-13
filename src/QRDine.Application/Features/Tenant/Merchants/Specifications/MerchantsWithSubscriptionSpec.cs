using QRDine.Application.Features.Tenant.Merchants.DTOs;
using QRDine.Application.Features.Tenant.Merchants.Extensions;
using QRDine.Domain.Tenant;

namespace QRDine.Application.Features.Tenant.Merchants.Specifications
{
    public class MerchantsWithSubscriptionSpec : Specification<Merchant, AdminMerchantDto>
    {
        public MerchantsWithSubscriptionSpec(string? searchKeyword, int pageNumber, int pageSize)
        {
            if (!string.IsNullOrWhiteSpace(searchKeyword))
            {
                Query.Where(m => m.Name.Contains(searchKeyword) ||
                                 (m.PhoneNumber != null && m.PhoneNumber.Contains(searchKeyword)));
            }

            Query.OrderByDescending(m => m.CreatedAt);

            Query.Skip((pageNumber - 1) * pageSize)
                 .Take(pageSize);

            Query.Select(MerchantExtensions.ToAdminMerchantDto);
        }
    }
}
