using QRDine.Domain.Catalog;

namespace QRDine.Application.Features.Catalog.Tables.Specifications
{
    public class TableByNameSpec : Specification<Table>
    {
        public TableByNameSpec(string name)
        {
            Query.Where(x => x.Name.ToLower() == name.ToLower());
        }
    }
}
