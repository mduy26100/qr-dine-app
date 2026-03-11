using QRDine.Domain.Enums;
using QRDine.Domain.Sales;

namespace QRDine.Application.Features.Sales.Orders.Specifications
{
    public class GetActiveOrderByTableSpec<TDto> : Specification<Order, TDto>, ISingleResultSpecification<Order, TDto>
    {
        public GetActiveOrderByTableSpec(
            Guid tableId, 
            Expression<Func<Order, TDto>> selector,
            Guid? merchantId = null, 
            Guid? sessionId = null)
        {
            if(merchantId.HasValue && sessionId.HasValue)
            {
                Query.Where(o => o.MerchantId == merchantId.Value && o.SessionId == sessionId.Value);
            }

            Query.Where(o => o.TableId == tableId
                          && o.Status != OrderStatus.Paid
                          && o.Status != OrderStatus.Cancelled
                          && !o.IsDeleted);

            Query.OrderByDescending(o => o.CreatedAt)
                 .Take(1);

            Query.Select(selector);
        }
    }
}
