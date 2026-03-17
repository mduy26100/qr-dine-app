using QRDine.Application.Features.Catalog.Categories.DTOs;
using QRDine.Application.Features.Catalog.Categories.Specifications;
using QRDine.Application.Features.Catalog.Repositories;

namespace QRDine.Application.Features.Catalog.Categories.Queries.GetCategoryProductLookup
{
    public class GetCategoryProductLookupQueryHandler : IRequestHandler<GetCategoryProductLookupQuery, List<CategoryLookupDto>>
    {
        private readonly ICategoryRepository _categoryRepository;

        public GetCategoryProductLookupQueryHandler(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public async Task<List<CategoryLookupDto>> Handle(GetCategoryProductLookupQuery request, CancellationToken cancellationToken)
        {
            var spec = new GetCategoryProductLookupSpec();
            return await _categoryRepository.ListAsync(spec, cancellationToken);
        }
    }
}
