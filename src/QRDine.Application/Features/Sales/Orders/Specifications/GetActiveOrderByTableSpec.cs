using QRDine.Application.Features.Sales.Orders.DTOs;
using QRDine.Application.Features.Sales.Orders.Extensions;
using QRDine.Domain.Enums;
using QRDine.Domain.Sales;

namespace QRDine.Application.Features.Sales.Orders.Specifications
{
    public class GetActiveOrderByTableSpec : Specification<Order, ManagementOrderDto>, ISingleResultSpecification<Order, ManagementOrderDto>
    {
        public GetActiveOrderByTableSpec(Guid tableId)
        {
            Query.Where(o => o.TableId == tableId
                          && o.Status != OrderStatus.Paid
                          && o.Status != OrderStatus.Cancelled
                          && !o.IsDeleted);

            Query.Select(OrderExtensions.ToManagementOrderDto);
        }
    }
}
