using QRDine.Application.Features.Sales.Orders.DTOs;

namespace QRDine.Application.Features.Sales.Orders.Queries.GetStorefrontOrder
{
    public record GetStorefrontOrderQuery(Guid MerchantId, Guid SessionId) : IRequest<StorefrontOrderDto?>;
}
