using QRDine.Application.Features.Sales.Orders.DTOs;

namespace QRDine.Application.Features.Sales.Orders.Queries.GetStorefrontActiveOrder
{
    public record GetStorefrontActiveOrderQuery(Guid MerchantId, Guid TableId, Guid SessionId) : IRequest<StorefrontOrderDto?>;
}
