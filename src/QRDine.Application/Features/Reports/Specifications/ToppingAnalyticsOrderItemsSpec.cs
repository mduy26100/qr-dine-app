using QRDine.Domain.Sales;

namespace QRDine.Application.Features.Reports.Specifications
{
    public class ToppingAnalyticsOrdersSpec : Specification<Order>
    {
        public ToppingAnalyticsOrdersSpec(Guid merchantId, DateTime startDate, DateTime endDate)
        {
            Query
                .Where(o => o.MerchantId == merchantId
                    && o.CreatedAt >= startDate
                    && o.CreatedAt <= endDate
                    && !o.IsDeleted)
                .Include(o => o.OrderItems);
        }
    }
}
