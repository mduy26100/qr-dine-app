using QRDine.Application.Features.Catalog.ToppingGroups.DTOs;
using QRDine.Domain.Catalog;

namespace QRDine.Application.Features.Catalog.ToppingGroups.Extensions
{
    public static class ToppingGroupExpressions
    {
        public static Expression<Func<ToppingGroup, ToppingGroupDto>> ToToppingGroupDto =>
            tg => new ToppingGroupDto
            {
                Id = tg.Id,
                Name = tg.Name,
                Description = tg.Description,
                IsRequired = tg.IsRequired,
                IsActive = tg.IsActive,

                ToppingCount = tg.Toppings.Count(t => !t.IsDeleted),

                AppliedProductCount = tg.ProductToppingGroups.Count
            };

        public static Expression<Func<ToppingGroup, ToppingGroupDetailDto>> ToToppingGroupDetailDto =>
            tg => new ToppingGroupDetailDto
            {
                Id = tg.Id,
                Name = tg.Name,
                Description = tg.Description,
                IsRequired = tg.IsRequired,
                MinSelections = tg.MinSelections,
                MaxSelections = tg.MaxSelections,
                IsActive = tg.IsActive,

                Toppings = tg.Toppings
                    .OrderBy(t => t.DisplayOrder)
                    .Select(t => new ToppingDto
                    {
                        Id = t.Id,
                        Name = t.Name,
                        Price = t.Price,
                        DisplayOrder = t.DisplayOrder,
                        IsAvailable = t.IsAvailable
                    }).ToList(),

                AppliedCategories = tg.ProductToppingGroups
                    .Where(ptg => !ptg.Product.IsDeleted)
                    .GroupBy(ptg => new
                    {
                        CategoryId = ptg.Product.Category.Id,
                        CategoryName = ptg.Product.Category.Name,
                        ParentName = ptg.Product.Category.Parent != null ? ptg.Product.Category.Parent.Name : null,
                        DisplayOrder = ptg.Product.Category.Parent != null ? ptg.Product.Category.Parent.DisplayOrder : ptg.Product.Category.DisplayOrder
                    })
                    .OrderBy(g => g.Key.DisplayOrder)
                    .Select(g => new AppliedCategoryDto
                    {
                        Id = g.Key.CategoryId,
                        Name = g.Key.ParentName != null ? g.Key.ParentName + " - " + g.Key.CategoryName : g.Key.CategoryName,

                        Products = g.Select(ptg => new AppliedProductDto
                        {
                            Id = ptg.ProductId,
                            Name = ptg.Product.Name
                        })
                        .OrderBy(p => p.Name)
                        .ToList()
                    })
                    .ToList()
            };
    }
}
