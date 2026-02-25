using QRDine.Domain.Catalog;

namespace QRDine.Application.Features.Catalog.Products.Specifications
{
    public class ProductsFilterCountSpec : Specification<Product>
    {
        public ProductsFilterCountSpec(string? searchTerm, Guid? categoryId, bool? isAvailable)
        {
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                Query.Where(x => x.Name.Contains(searchTerm));
            }

            if (categoryId.HasValue)
            {
                Query.Where(x => x.CategoryId == categoryId.Value);
            }

            if (isAvailable.HasValue)
            {
                Query.Where(x => x.IsAvailable == isAvailable.Value);
            }
        }
    }

    public class ProductsFilterPagedSpec : Specification<Product>
    {
        public ProductsFilterPagedSpec(string? searchTerm, Guid? categoryId, bool? isAvailable, int pageNumber, int pageSize)
        {
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                Query.Where(x => x.Name.Contains(searchTerm));
            }

            if (categoryId.HasValue)
            {
                Query.Where(x => x.CategoryId == categoryId.Value);
            }

            if (isAvailable.HasValue)
            {
                Query.Where(x => x.IsAvailable == isAvailable.Value);
            }

            Query.Include(x => x.Category)
                 .ThenInclude(c => c.Parent);

            Query.OrderByDescending(x => x.CreatedAt)
                 .Skip((pageNumber - 1) * pageSize)
                 .Take(pageSize);
        }
    }
}
