using QRDine.Domain.Catalog;

namespace QRDine.Application.Features.Catalog.Tables.Specifications
{
    public class TableByNameSpec : Specification<Table>
    {
        public TableByNameSpec(string name, Guid? excludeId = null)
        {
            Query.Where(x => x.Name.ToLower() == name.ToLower());

            if (excludeId.HasValue)
            {
                Query.Where(x => x.Id != excludeId.Value);
            }
        }
    }
}
