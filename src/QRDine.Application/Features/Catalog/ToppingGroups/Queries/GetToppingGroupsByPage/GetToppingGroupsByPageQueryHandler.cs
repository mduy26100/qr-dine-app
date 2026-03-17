using QRDine.Application.Common.Models;
using QRDine.Application.Features.Catalog.Repositories;
using QRDine.Application.Features.Catalog.ToppingGroups.DTOs;
using QRDine.Application.Features.Catalog.ToppingGroups.Specifications;

namespace QRDine.Application.Features.Catalog.ToppingGroups.Queries.GetToppingGroupsByPage
{
    public class GetToppingGroupsByPageQueryHandler : IRequestHandler<GetToppingGroupsByPageQuery, PagedResult<ToppingGroupDto>>
    {
        private readonly IToppingGroupRepository _toppingGroupRepository;

        public GetToppingGroupsByPageQueryHandler(IToppingGroupRepository toppingGroupRepository)
        {
            _toppingGroupRepository = toppingGroupRepository;
        }

        public async Task<PagedResult<ToppingGroupDto>> Handle(GetToppingGroupsByPageQuery request, CancellationToken cancellationToken)
        {
            var countSpec = new ToppingGroupsCountSpec(request.Keyword);
            var totalCount = await _toppingGroupRepository.CountAsync(countSpec, cancellationToken);

            var pagedSpec = new ToppingGroupsByPageSpec(request.PageNumber, request.PageSize, request.Keyword);
            var toppingGroups = await _toppingGroupRepository.ListAsync(pagedSpec, cancellationToken);

            return new PagedResult<ToppingGroupDto>(toppingGroups, totalCount, request.PageNumber, request.PageSize);
        }
    }
}
