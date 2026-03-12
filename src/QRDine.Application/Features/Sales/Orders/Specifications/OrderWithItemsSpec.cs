using QRDine.Domain.Sales;

namespace QRDine.Application.Features.Sales.Orders.Specifications
{
    public class OrderWithItemsSpec : SingleResultSpecification<Order>
    {
        public OrderWithItemsSpec(Guid orderId)
        {
            Query.Where(o => o.Id == orderId)
                 .Include(o => o.OrderItems);
        }
    }
}
