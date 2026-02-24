using QRDine.Domain.Catalog;

namespace QRDine.Application.Features.Catalog.Categories.Specifications
{
    public class CategoryHasChildrenSpec : Specification<Category>
    {
        public CategoryHasChildrenSpec(Guid targetCategoryId)
        {
            Query.Where(x => x.ParentId == targetCategoryId);
        }
    }
}
