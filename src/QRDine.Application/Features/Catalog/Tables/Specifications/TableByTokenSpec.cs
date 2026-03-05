using QRDine.Domain.Catalog;

namespace QRDine.Application.Features.Catalog.Tables.Specifications
{
    public class TableByTokenSpec : Specification<Table>, ISingleResultSpecification<Table>
    {
        public TableByTokenSpec(Guid merchantId, string qrCodeToken)
        {
            Query.Where(t => t.MerchantId == merchantId && t.QrCodeToken == qrCodeToken && !t.IsDeleted);
        }
    }
}
