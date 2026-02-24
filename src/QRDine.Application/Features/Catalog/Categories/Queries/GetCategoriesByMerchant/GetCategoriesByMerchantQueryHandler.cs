using QRDine.Application.Features.Catalog.Categories.DTOs;
using QRDine.Application.Features.Catalog.Categories.Extensions;
using QRDine.Application.Features.Catalog.Categories.Specifications;
using QRDine.Application.Features.Catalog.Repositories;

namespace QRDine.Application.Features.Catalog.Categories.Queries.GetCategoriesByMerchant
{
    public class GetCategoriesByMerchantQueryHandler : IRequestHandler<GetCategoriesByMerchantQuery, List<CategoryTreeDto>>
    {
        private readonly ICategoryRepository _repository;
        private readonly IMapper _mapper;

        public GetCategoriesByMerchantQueryHandler(ICategoryRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<List<CategoryTreeDto>> Handle(GetCategoriesByMerchantQuery request, CancellationToken cancellationToken)
        {
            var spec = new CategoriesByMerchantSpec(request.MerchantId);
            var categories = await _repository.ListAsync(spec, cancellationToken);

            var categoryDtos = _mapper.Map<List<CategoryTreeDto>>(categories);

            return categoryDtos.BuildTree();
        }
    }
}
