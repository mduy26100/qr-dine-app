using QRDine.Application.Features.Sales.Orders.DTOs;
using QRDine.Application.Features.Sales.Orders.Extensions;
using QRDine.Domain.Enums;
using QRDine.Domain.Sales;

namespace QRDine.Application.Features.Sales.Orders.Specifications
{
    public class OrderHistoryByPageSpec : Specification<Order, OrderListDto>
    {
        public OrderHistoryByPageSpec(string? searchTerm, int pageNumber, int pageSize)
        {
            Query.Where(o => o.Status == OrderStatus.Paid || o.Status == OrderStatus.Cancelled);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                Query.Where(o => o.OrderCode.Contains(searchTerm) || o.TableName.Contains(searchTerm));
            }

            Query.OrderByDescending(o => o.CreatedAt)
                 .ThenByDescending(o => o.Id);

            Query.Skip((pageNumber - 1) * pageSize)
                 .Take(pageSize);

            Query.Select(OrderExtensions.ToOrderListDto);
        }
    }
}
