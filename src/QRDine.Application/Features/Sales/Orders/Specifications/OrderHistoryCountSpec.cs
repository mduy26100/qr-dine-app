using QRDine.Domain.Enums;
using QRDine.Domain.Sales;

namespace QRDine.Application.Features.Sales.Orders.Specifications
{
    public class OrderHistoryCountSpec : Specification<Order>
    {
        public OrderHistoryCountSpec(string? searchTerm)
        {
            Query.Where(o => o.Status == OrderStatus.Paid || o.Status == OrderStatus.Cancelled);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                Query.Where(o => o.OrderCode.Contains(searchTerm) || o.TableName.Contains(searchTerm));
            }
        }
    }
}
