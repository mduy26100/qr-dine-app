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
    }
}
