using QRDine.Domain.Catalog;

namespace QRDine.Application.Features.Catalog.Categories.Specifications
{
    public class CategoryByNameSpec : Specification<Category>
    {
        public CategoryByNameSpec(Guid merchantId, string name)
        {
            Query.Where(x =>
                x.MerchantId == merchantId &&
                x.Name == name);
        }
    }
}
