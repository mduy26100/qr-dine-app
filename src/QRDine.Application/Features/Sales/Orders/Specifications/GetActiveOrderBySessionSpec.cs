using QRDine.Domain.Enums;
using QRDine.Domain.Sales;

namespace QRDine.Application.Features.Sales.Orders.Specifications
{
    public class GetActiveOrderBySessionSpec : Specification<Order>, ISingleResultSpecification<Order>
    {
        public GetActiveOrderBySessionSpec(Guid merchantId, Guid tableId, Guid sessionId)
        {
            Query.Where(o => o.MerchantId == merchantId
                          && o.TableId == tableId
                          && o.SessionId == sessionId
                          && o.Status != OrderStatus.Paid
                          && o.Status != OrderStatus.Cancelled
                          && !o.IsDeleted)
                 .Include(o => o.OrderItems);
        }
    }
}
