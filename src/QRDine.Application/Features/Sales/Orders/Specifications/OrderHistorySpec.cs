using QRDine.Application.Features.Sales.Orders.DTOs;
using QRDine.Application.Features.Sales.Orders.Extensions;
using QRDine.Domain.Enums;
using QRDine.Domain.Sales;

namespace QRDine.Application.Features.Sales.Orders.Specifications
{
    public class OrderHistorySpec : Specification<Order, OrderListDto>
    {
        public OrderHistorySpec()
        {
            Query.Where(o => o.Status == OrderStatus.Paid || o.Status == OrderStatus.Cancelled)
                 .OrderByDescending(o => o.CreatedAt);

            Query.Select(OrderExtensions.ToOrderListDto);
        }
    }
}
