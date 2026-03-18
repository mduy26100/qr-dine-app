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
                    .Where(p => p.IsAvailable)
                    .Select(p => new StorefrontMenuProductDto
                    {
                        Id = p.Id,
                        Name = p.Name,
                        Description = p.Description,
                        ImageUrl = p.ImageUrl,
                        Price = p.Price,
                        ToppingGroups = p.ProductToppingGroups
                            .Where(ptg => ptg.ToppingGroup.IsActive)
                            .Select(ptg => new StorefrontMenuToppingGroupDto
                            {
                                Id = ptg.ToppingGroup.Id,
                                Name = ptg.ToppingGroup.Name,
                                Description = ptg.ToppingGroup.Description,
                                IsRequired = ptg.ToppingGroup.IsRequired,
                                MinSelections = ptg.ToppingGroup.MinSelections,
                                MaxSelections = ptg.ToppingGroup.MaxSelections,
                                Toppings = ptg.ToppingGroup.Toppings
                                    .Where(t => t.IsAvailable)
                                    .OrderBy(t => t.DisplayOrder)
                                    .Select(t => new StorefrontMenuToppingDto
                                    {
                                        Id = t.Id,
                                        Name = t.Name,
                                        Price = t.Price
                                    }).ToList()
                            }).ToList()
                    }).ToList()
            };

        public static Expression<Func<Category, CategoryLookupDto>> ToLookupDto =>
            c => new CategoryLookupDto
            {
                Id = c.Id,
                Name = c.Parent != null ? c.Parent.Name + " - " + c.Name : c.Name,
                DisplayOrder = c.Parent != null ? c.Parent.DisplayOrder : c.DisplayOrder,

                Products = c.Products
                    .Where(p => !p.IsDeleted)
                    .Select(p => new ProductLookupDto
                    {
                        Id = p.Id,
                        Name = p.Name
                    }).ToList()
            };
    }
}
