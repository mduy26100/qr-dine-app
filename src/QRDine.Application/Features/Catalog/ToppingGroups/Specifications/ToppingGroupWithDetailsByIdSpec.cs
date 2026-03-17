using QRDine.Domain.Catalog;

namespace QRDine.Application.Features.Catalog.ToppingGroups.Specifications
{
    public class ToppingGroupWithDetailsByIdSpec : Specification<ToppingGroup>, ISingleResultSpecification<ToppingGroup>
    {
        public ToppingGroupWithDetailsByIdSpec(Guid id)
        {
            Query.Where(tg => tg.Id == id)
                 .Include(tg => tg.Toppings)
                 .Include(tg => tg.ProductToppingGroups);

            Query.IgnoreQueryFilters(); 
        }
    }
}
