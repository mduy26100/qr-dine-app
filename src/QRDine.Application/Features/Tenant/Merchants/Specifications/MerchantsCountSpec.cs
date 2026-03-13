using QRDine.Domain.Tenant;

namespace QRDine.Application.Features.Tenant.Merchants.Specifications
{
    public class MerchantsCountSpec : Specification<Merchant>
    {
        public MerchantsCountSpec(string? searchKeyword)
        {
            if (!string.IsNullOrWhiteSpace(searchKeyword))
            {
                Query.Where(m => m.Name.Contains(searchKeyword) ||
                                 (m.PhoneNumber != null && m.PhoneNumber.Contains(searchKeyword)));
            }
        }
    }
}
