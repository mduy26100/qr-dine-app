using QRDine.Application.Features.Catalog.ToppingGroups.DTOs;
using QRDine.Application.Features.Catalog.ToppingGroups.Extensions;
using QRDine.Domain.Catalog;

namespace QRDine.Application.Features.Catalog.ToppingGroups.Specifications
{
    public class ToppingGroupDetailSpec : Specification<ToppingGroup, ToppingGroupDetailDto>, ISingleResultSpecification<ToppingGroup, ToppingGroupDetailDto>
    {
        public ToppingGroupDetailSpec(Guid id)
        {
            Query.Where(tg => tg.Id == id);

            Query.Select(ToppingGroupExpressions.ToToppingGroupDetailDto);
        }
    }
}
