using QRDine.Application.Common.Abstractions.Caching;
using QRDine.Application.Common.Constants;
using QRDine.Application.Features.Catalog.Categories.DTOs;
using QRDine.Application.Features.Catalog.Categories.Specifications;
using QRDine.Application.Features.Catalog.Repositories;

namespace QRDine.Application.Features.Catalog.Categories.Queries.GetStorefrontMenu
{
    public class GetStorefrontMenuQueryHandler : IRequestHandler<GetStorefrontMenuQuery, List<StorefrontMenuCategoryDto>>
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly ICacheService _cacheService;

        public GetStorefrontMenuQueryHandler(ICategoryRepository categoryRepository, ICacheService cacheService)
        {
            _categoryRepository = categoryRepository;
            _cacheService = cacheService;
        }

        public async Task<List<StorefrontMenuCategoryDto>> Handle(GetStorefrontMenuQuery request, CancellationToken cancellationToken)
        {
            var cacheKey = CacheKeys.StorefrontMenu(request.MerchantId);

            var cachedMenu = await _cacheService.GetAsync<List<StorefrontMenuCategoryDto>>(cacheKey, cancellationToken);

            if (cachedMenu != null)
            {
                return cachedMenu;
            }

            var spec = new GetStorefrontMenuSpec(request.MerchantId);
            var menu = await _categoryRepository.ListAsync(spec, cancellationToken);

            await _cacheService.SetAsync(cacheKey, menu, CacheDurations.StorefrontMenu, cancellationToken);

            return menu;
        }
    }
}
