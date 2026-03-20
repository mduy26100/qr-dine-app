using QRDine.Application.Features.Catalog.Products.DTOs;
using QRDine.Domain.Catalog;

namespace QRDine.Application.Features.Catalog.Products.Extensions
{
    public static class ProductWithToppingsExtensions
    {
        public static Expression<Func<Product, ProductWithToppingsDto>> ToProductWithToppingsDto =>
            p => new ProductWithToppingsDto
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price,
                IsAvailable = p.IsAvailable,
                ToppingGroups = p.ProductToppingGroups
                    .Where(ptg => !ptg.ToppingGroup.IsDeleted)
                    .Select(ptg => new ProductToppingGroupDto
                    {
                        Id = ptg.ToppingGroup.Id,
                        Name = ptg.ToppingGroup.Name,
                        IsRequired = ptg.ToppingGroup.IsRequired,
                        MinSelections = ptg.ToppingGroup.MinSelections,
                        MaxSelections = ptg.ToppingGroup.MaxSelections,
                        IsActive = ptg.ToppingGroup.IsActive,
                        Toppings = ptg.ToppingGroup.Toppings
                            .Where(t => !t.IsDeleted)
                            .Select(t => new ProductToppingDto
                            {
                                Id = t.Id,
                                ToppingGroupId = t.ToppingGroupId,
                                Name = t.Name,
                                Price = t.Price,
                                IsAvailable = t.IsAvailable
                            }).ToList()
                    }).ToList()
            };
    }
}
