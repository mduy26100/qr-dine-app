using QRDine.Application.Common.Abstractions.Identity;
using QRDine.Application.Features.Catalog.Categories.DTOs;
using QRDine.Application.Features.Catalog.Categories.Specifications;
using QRDine.Application.Features.Catalog.Repositories;

namespace QRDine.Application.Features.Catalog.Categories.Queries.GetCategoriesByMerchant
{
    public class GetCategoriesByMerchantQueryHandler : IRequestHandler<GetCategoriesByMerchantQuery, List<CategoryTreeDto>>
    {
        private readonly ICategoryRepository _repository;
        private readonly IMapper _mapper;
        private readonly ICurrentUserService _currentUserService;

        public GetCategoriesByMerchantQueryHandler(
            ICategoryRepository repository,
            IMapper mapper,
            ICurrentUserService currentUserService)
        {
            _repository = repository;
            _mapper = mapper;
            _currentUserService = currentUserService;
        }

        public async Task<List<CategoryTreeDto>> Handle(GetCategoriesByMerchantQuery request, CancellationToken cancellationToken)
        {
            var targetMerchantId = request.MerchantId ?? _currentUserService.MerchantId!.Value;

            var spec = new CategoriesByMerchantSpec(targetMerchantId);
            var categories = await _repository.ListAsync(spec, cancellationToken);

            var categoryDtos = _mapper.Map<List<CategoryTreeDto>>(categories);

            var childrenLookup = categoryDtos
                .Where(c => c.ParentId != null)
                .ToLookup(c => c.ParentId);

            var rootCategories = categoryDtos
                .Where(c => c.ParentId == null)
                .ToList();

            foreach (var root in rootCategories)
            {
                root.Children = childrenLookup[root.Id].ToList();
            }

            return rootCategories;
        }
    }
}
