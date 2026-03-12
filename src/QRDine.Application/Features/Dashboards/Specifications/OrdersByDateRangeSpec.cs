using QRDine.Domain.Sales;

namespace QRDine.Application.Features.Dashboards.Specifications
{
    public class OrdersByDateRangeSpec : Specification<Order>
    {
        public OrdersByDateRangeSpec(DateTime startDate, DateTime endDate)
        {
            Query.Where(o => o.CreatedAt >= startDate && o.CreatedAt <= endDate);
        }
    }
}
