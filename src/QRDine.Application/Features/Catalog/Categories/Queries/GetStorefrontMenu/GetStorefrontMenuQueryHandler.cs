using QRDine.Application.Features.Catalog.Categories.DTOs;
using QRDine.Application.Features.Catalog.Categories.Specifications;
using QRDine.Application.Features.Catalog.Repositories;

namespace QRDine.Application.Features.Catalog.Categories.Queries.GetStorefrontMenu
{
    public class GetStorefrontMenuQueryHandler : IRequestHandler<GetStorefrontMenuQuery, List<StorefrontMenuCategoryDto>>
    {
        private readonly ICategoryRepository _categoryRepository;

        public GetStorefrontMenuQueryHandler(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public async Task<List<StorefrontMenuCategoryDto>> Handle(GetStorefrontMenuQuery request, CancellationToken cancellationToken)
        {
            var spec = new GetStorefrontMenuSpec(request.MerchantId);

            var menu = await _categoryRepository.ListAsync(spec, cancellationToken);

            return menu;
        }
    }
}
