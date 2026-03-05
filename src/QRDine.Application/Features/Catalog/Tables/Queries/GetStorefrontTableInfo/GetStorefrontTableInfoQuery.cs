using QRDine.Application.Features.Catalog.Tables.DTOs;

namespace QRDine.Application.Features.Catalog.Tables.Queries.GetStorefrontTableInfo
{
    public record GetStorefrontTableInfoQuery(Guid MerchantId, string QrCodeToken) : IRequest<StorefrontTableInfoDto>;
}
