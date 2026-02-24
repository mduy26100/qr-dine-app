using QRDine.Domain.Catalog;

namespace QRDine.Application.Features.Catalog.Categories.Specifications
{
    public class CategoryNameConflictSpec : Specification<Category>
    {
        public CategoryNameConflictSpec(Guid merchantId, string name, Guid? parentId, Guid excludeCategoryId)
        {
            Query.Where(x =>
                x.MerchantId == merchantId &&
                x.Name == name &&
                x.ParentId == parentId &&
                x.Id != excludeCategoryId);
        }
    }
}
