using QRDine.Domain.Sales;

namespace QRDine.Application.Features.Dashboards.Specifications
{
    public class OrdersByDateRangeSpec : Specification<Order>
    {
        public OrdersByDateRangeSpec(Guid merchantId, DateTime startDate, DateTime endDate)
        {
            Query.Where(o => o.MerchantId == merchantId
                          && o.CreatedAt >= startDate
                          && o.CreatedAt <= endDate);
        }
    }
}
