using QRDine.Domain.Catalog;

namespace QRDine.Application.Features.Catalog.Tables.Specifications
{
    public class TableByIdAndMerchantSpec : Specification<Table>, ISingleResultSpecification<Table>
    {
        public TableByIdAndMerchantSpec(Guid tableId, Guid merchantId)
        {
            Query.Where(t => t.Id == tableId && t.MerchantId == merchantId);
        }
    }
}
