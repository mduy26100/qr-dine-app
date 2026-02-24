using QRDine.Domain.Catalog;

namespace QRDine.Application.Features.Catalog.Categories.Specifications
{
    public class AllCategoriesOrderedSpec : Specification<Category>
    {
        public AllCategoriesOrderedSpec()
        {
            Query.OrderBy(x => x.DisplayOrder);
        }
    }
}
