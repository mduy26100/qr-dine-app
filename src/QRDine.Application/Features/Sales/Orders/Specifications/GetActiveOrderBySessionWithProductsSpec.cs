using QRDine.Application.Features.Sales.Orders.DTOs;
using QRDine.Application.Features.Sales.Orders.Extensions;
using QRDine.Domain.Enums;
using QRDine.Domain.Sales;

namespace QRDine.Application.Features.Sales.Orders.Specifications
{
    public class GetActiveOrderBySessionWithProductsSpec : Specification<Order, StorefrontOrderDto>, ISingleResultSpecification<Order, StorefrontOrderDto>
    {
        public GetActiveOrderBySessionWithProductsSpec(Guid merchantId, Guid sessionId)
        {
            Query.Where(o => o.MerchantId == merchantId
                          && o.SessionId == sessionId
                          && o.Status != OrderStatus.Paid
                          && o.Status != OrderStatus.Cancelled
                          && !o.IsDeleted);

            Query.Select(OrderExtensions.ToStorefrontOrderDto);
        }
    }
}
