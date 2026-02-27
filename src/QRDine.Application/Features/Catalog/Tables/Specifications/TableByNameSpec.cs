using QRDine.Domain.Catalog;

namespace QRDine.Application.Features.Catalog.Tables.Specifications
{
    public class TableByNameSpec : Specification<Table>
    {
        public TableByNameSpec(string name, Guid? merchantId = null, Guid? excludeId = null, bool includeDeleted = false)
        {
            Query.Where(x => x.Name.ToLower() == name.ToLower());

            if (merchantId.HasValue)
            {
                Query.Where(x => x.MerchantId == merchantId.Value);
            }

            if (excludeId.HasValue)
            {
                Query.Where(x => x.Id != excludeId.Value);
            }

            if (includeDeleted)
            {
                Query.IgnoreQueryFilters();
            }
        }
    }
}
