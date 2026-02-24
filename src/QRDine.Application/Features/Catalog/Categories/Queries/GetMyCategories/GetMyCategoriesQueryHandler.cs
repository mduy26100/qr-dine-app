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

        public GetMyCategoriesQueryHandler(ICategoryRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<List<CategoryTreeDto>> Handle(GetMyCategoriesQuery request, CancellationToken cancellationToken)
        {
            var spec = new AllCategoriesOrderedSpec();
            var categories = await _repository.ListAsync(spec, cancellationToken);

            var categoryDtos = _mapper.Map<List<CategoryTreeDto>>(categories);

            return categoryDtos.BuildTree();
        }
    }
}
