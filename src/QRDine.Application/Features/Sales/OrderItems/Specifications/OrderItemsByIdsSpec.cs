using QRDine.Domain.Sales;

namespace QRDine.Application.Features.Sales.OrderItems.Specifications
{
    public class OrderItemsByIdsSpec : Specification<OrderItem>
    {
        public OrderItemsByIdsSpec(List<Guid> itemIds)
        {
            Query.Where(x => itemIds.Contains(x.Id))
                 .Include(x => x.Order);
        }
    }
}
