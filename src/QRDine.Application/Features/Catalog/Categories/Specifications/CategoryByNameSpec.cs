using QRDine.Domain.Catalog;

namespace QRDine.Application.Features.Catalog.Categories.Specifications
{
    public class CategoryByNameSpec : Specification<Category>
    {
        public CategoryByNameSpec(string name, Guid? excludeId = null)
        {
            Query.Where(x => x.Name == name);

            if (excludeId.HasValue)
            {
                Query.Where(x => x.Id != excludeId.Value);
            }
        }
    }
}
