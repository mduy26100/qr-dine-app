using QRDine.Application.Features.Catalog.Tables.DTOs;
using QRDine.Application.Features.Catalog.Tables.Extensions;
using QRDine.Domain.Catalog;

namespace QRDine.Application.Features.Catalog.Tables.Specifications
{
    public class TableByTokenSpec : Specification<Table, StorefrontTableInfoDto>, ISingleResultSpecification<Table, StorefrontTableInfoDto>
    {
        public TableByTokenSpec(Guid merchantId, string qrCodeToken)
        {
            Query.Where(t => t.MerchantId == merchantId && t.QrCodeToken == qrCodeToken && !t.IsDeleted);
            Query.Select(TableExpressions.ToStorefrontTableInfoDto);
        }
    }
}
