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

                AppliedProductIds = tg.ProductToppingGroups
                    .Select(ptg => ptg.ProductId)
                    .ToList()
            };
    }
}
