using QRDine.Application.Features.Catalog.Products.DTOs;
using QRDine.Domain.Catalog;

namespace QRDine.Application.Features.Catalog.Products.Specifications
{
    public class ProductsFilterCountSpec : Specification<Product>
    {
        public ProductsFilterCountSpec(string? searchTerm, Guid? categoryId, bool? isAvailable)
        {
            if (!string.IsNullOrWhiteSpace(searchTerm))
                Query.Where(x => x.Name.Contains(searchTerm));

            if (categoryId.HasValue)
                Query.Where(x => x.CategoryId == categoryId.Value);

            if (isAvailable.HasValue)
                Query.Where(x => x.IsAvailable == isAvailable.Value);
        }
    }

    public class ProductsByPageSpec : Specification<Product, ProductDto>
    {
        public ProductsByPageSpec(string? searchTerm, Guid? categoryId, bool? isAvailable, int pageNumber, int pageSize)
        {
            if (!string.IsNullOrWhiteSpace(searchTerm))
                Query.Where(x => x.Name.Contains(searchTerm));

            if (categoryId.HasValue)
                Query.Where(x => x.CategoryId == categoryId.Value);

            if (isAvailable.HasValue)
                Query.Where(x => x.IsAvailable == isAvailable.Value);

            Query.OrderByDescending(x => x.CreatedAt)
                 .ThenByDescending(x => x.Id);

            Query.Skip((pageNumber - 1) * pageSize)
                 .Take(pageSize);

            Query.Select(x => new ProductDto
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description,
                ImageUrl = x.ImageUrl,
                Price = x.Price,
                IsAvailable = x.IsAvailable,
                CategoryId = x.CategoryId,
                CategoryName = x.Category.Name,
                ParentCategoryName = x.Category.Parent != null ? x.Category.Parent.Name : null,
                CreatedAt = x.CreatedAt
            });
        }
    }

    public class ProductsByCursorSpec : Specification<Product, ProductDto>
    {
        public ProductsByCursorSpec(string? searchTerm, Guid? categoryId, bool? isAvailable, DateTime? cursorCreatedAt, Guid? cursorId, int limit)
        {
            if (!string.IsNullOrWhiteSpace(searchTerm))
                Query.Where(x => x.Name.Contains(searchTerm));

            if (categoryId.HasValue)
                Query.Where(x => x.CategoryId == categoryId.Value);

            if (isAvailable.HasValue)
                Query.Where(x => x.IsAvailable == isAvailable.Value);

            Query.OrderByDescending(x => x.CreatedAt)
                 .ThenByDescending(x => x.Id);

            if (cursorCreatedAt.HasValue && cursorId.HasValue)
            {
                Query.Where(x => x.CreatedAt < cursorCreatedAt.Value ||
                                (x.CreatedAt == cursorCreatedAt.Value && x.Id.CompareTo(cursorId.Value) < 0));
            }
            else if (cursorCreatedAt.HasValue)
            {
                Query.Where(x => x.CreatedAt < cursorCreatedAt.Value);
            }

            Query.Take(limit);

            Query.Select(x => new ProductDto
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description,
                ImageUrl = x.ImageUrl,
                Price = x.Price,
                IsAvailable = x.IsAvailable,
                CategoryId = x.CategoryId,
                CategoryName = x.Category.Name,
                ParentCategoryName = x.Category.Parent != null ? x.Category.Parent.Name : null,
                CreatedAt = x.CreatedAt
            });
        }
    }
}