using QRDine.Domain.Catalog;

namespace QRDine.Application.Features.Catalog.ToppingGroups.Specifications
{
    public class ToppingGroupsCountSpec : Specification<ToppingGroup>
    {
        public ToppingGroupsCountSpec(string? keyword)
        {
            if (!string.IsNullOrEmpty(keyword))
            {
                Query.Where(tg => tg.Name.Contains(keyword) ||
                                 (tg.Description != null && tg.Description.Contains(keyword)));
            }
        }
    }
}
