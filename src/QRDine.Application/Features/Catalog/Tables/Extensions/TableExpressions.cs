using QRDine.Application.Features.Catalog.Tables.DTOs;
using QRDine.Domain.Catalog;
using System.Linq.Expressions;

namespace QRDine.Application.Features.Catalog.Tables.Extensions
{
    public static class TableExpressions
    {
        public static Expression<Func<Table, StorefrontTableInfoDto>> ToStorefrontTableInfoDto =>
            t => new StorefrontTableInfoDto
            {
                TableId = t.Id,
                MerchantId = t.MerchantId,
                TableName = t.Name,
                IsOccupied = t.IsOccupied,

                SessionId = t.IsOccupied && t.CurrentSessionId.HasValue ? t.CurrentSessionId.Value : Guid.Empty,

                MerchantName = t.Merchant.Name,
                MerchantAddress = t.Merchant.Address,
                MerchantLogoUrl = t.Merchant.LogoUrl
            };
    }
}
