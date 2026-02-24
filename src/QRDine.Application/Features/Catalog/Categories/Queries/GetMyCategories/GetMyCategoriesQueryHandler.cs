using QRDine.Application.Common.Abstractions.Identity;
using QRDine.Application.Features.Catalog.Categories.DTOs;
using QRDine.Application.Features.Catalog.Categories.Extensions;
using QRDine.Application.Features.Catalog.Categories.Specifications;
using QRDine.Application.Features.Catalog.Repositories;

namespace QRDine.Application.Features.Catalog.Categories.Queries.GetMyCategories
{
    public class GetMyCategoriesQueryHandler : IRequestHandler<GetMyCategoriesQuery, List<CategoryTreeDto>>
    {
        private readonly ICategoryRepository _repository;
        private readonly IMapper _mapper;
        private readonly ICurrentUserService _currentUserService;

        public GetMyCategoriesQueryHandler(
            ICategoryRepository repository,
            IMapper mapper,
            ICurrentUserService currentUserService)
        {
            _repository = repository;
            _mapper = mapper;
            _currentUserService = currentUserService;
        }

        public async Task<List<CategoryTreeDto>> Handle(GetMyCategoriesQuery request, CancellationToken cancellationToken)
        {
            var merchantId = _currentUserService.MerchantId
                ?? throw new UnauthorizedAccessException("Merchant context is missing.");

            var spec = new CategoriesByMerchantSpec(merchantId);
            var categories = await _repository.ListAsync(spec, cancellationToken);

            var categoryDtos = _mapper.Map<List<CategoryTreeDto>>(categories);

            return categoryDtos.BuildTree();
        }
    }
}
