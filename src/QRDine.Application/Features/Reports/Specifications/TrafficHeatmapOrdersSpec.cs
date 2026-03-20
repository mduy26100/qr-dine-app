using QRDine.Domain.Sales;

namespace QRDine.Application.Features.Reports.Specifications
{
    public class TrafficHeatmapOrdersSpec : Specification<Order>
    {
        public TrafficHeatmapOrdersSpec(Guid merchantId, DateTime startDate, DateTime endDate)
        {
            Query
                .Where(o => o.MerchantId == merchantId
                    && o.CreatedAt >= startDate
                    && o.CreatedAt <= endDate
                    && !o.IsDeleted);
        }
    }
}
