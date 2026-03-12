using QRDine.Application.Features.Sales.Orders.DTOs;
using QRDine.Application.Features.Sales.Orders.Extensions;
using QRDine.Domain.Sales;

namespace QRDine.Application.Features.Sales.Orders.Specifications
{
    public class OrderDetailSpec : SingleResultSpecification<Order, ManagementOrderDto>
    {
        public OrderDetailSpec(Guid orderId)
        {
            Query.Where(o => o.Id == orderId);

            Query.Select(OrderExtensions.ToManagementOrderDto);
        }
    }
}
