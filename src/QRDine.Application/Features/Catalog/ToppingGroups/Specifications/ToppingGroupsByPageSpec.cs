using QRDine.Application.Features.Catalog.ToppingGroups.DTOs;
using QRDine.Application.Features.Catalog.ToppingGroups.Extensions;
using QRDine.Domain.Catalog;

namespace QRDine.Application.Features.Catalog.ToppingGroups.Specifications
{
    public class ToppingGroupsByPageSpec : Specification<ToppingGroup, ToppingGroupDto>
    {
        public ToppingGroupsByPageSpec(int pageNumber, int pageSize, string? keyword)
        {
            if (!string.IsNullOrEmpty(keyword))
            {
                Query.Where(tg => tg.Name.Contains(keyword) ||
                                 (tg.Description != null && tg.Description.Contains(keyword)));
            }

            Query.OrderByDescending(tg => tg.CreatedAt)
                 .ThenByDescending(tg => tg.Id);

            Query.Skip((pageNumber - 1) * pageSize)
                 .Take(pageSize);

            Query.Select(ToppingGroupExpressions.ToToppingGroupDto);
        }
    }
}
