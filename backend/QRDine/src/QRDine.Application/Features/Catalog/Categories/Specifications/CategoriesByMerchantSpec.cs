using QRDine.Domain.Catalog;

namespace QRDine.Application.Features.Catalog.Categories.Specifications
{
    public class CategoriesByMerchantSpec : Specification<Category>
    {
        public CategoriesByMerchantSpec(Guid merchantId)
        {
            Query.Where(x => x.MerchantId == merchantId)
                 .OrderBy(x => x.DisplayOrder);
        }
    }
}
