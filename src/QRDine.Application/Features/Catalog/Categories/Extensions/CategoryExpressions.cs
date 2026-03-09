using QRDine.Application.Features.Catalog.Categories.DTOs;
using QRDine.Domain.Catalog;

namespace QRDine.Application.Features.Catalog.Categories.Extensions
{
    public static class CategoryExpressions
    {
        public static Expression<Func<Category, StorefrontMenuCategoryDto>> ToStorefrontMenuDto =>
            c => new StorefrontMenuCategoryDto
            {
                Id = c.Id,
                Name = c.Parent != null ? c.Parent.Name + " - " + c.Name : c.Name,

                DisplayOrder = c.Parent != null ? c.Parent.DisplayOrder : c.DisplayOrder,

                Products = c.Products
                    .Where(p => p.IsAvailable && !p.IsDeleted)
                    .Select(p => new StorefrontMenuProductDto
                    {
                        Id = p.Id,
                        Name = p.Name,
                        Description = p.Description,
                        ImageUrl = p.ImageUrl,
                        Price = p.Price
                    }).ToList()
            };
    }
}
