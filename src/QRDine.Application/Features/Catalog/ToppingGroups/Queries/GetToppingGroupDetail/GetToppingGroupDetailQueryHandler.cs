using QRDine.Application.Common.Exceptions;
using QRDine.Application.Features.Catalog.Repositories;
using QRDine.Application.Features.Catalog.ToppingGroups.DTOs;
using QRDine.Application.Features.Catalog.ToppingGroups.Specifications;

namespace QRDine.Application.Features.Catalog.ToppingGroups.Queries.GetToppingGroupDetail
{
    public class GetToppingGroupDetailQueryHandler : IRequestHandler<GetToppingGroupDetailQuery, ToppingGroupDetailDto>
    {
        private readonly IToppingGroupRepository _toppingGroupRepository;

        public GetToppingGroupDetailQueryHandler(IToppingGroupRepository toppingGroupRepository)
        {
            _toppingGroupRepository = toppingGroupRepository;
        }

        public async Task<ToppingGroupDetailDto> Handle(GetToppingGroupDetailQuery request, CancellationToken cancellationToken)
        {
            var spec = new ToppingGroupDetailSpec(request.Id);

            var toppingGroup = await _toppingGroupRepository.SingleOrDefaultAsync(spec, cancellationToken);

            if (toppingGroup == null)
            {
                throw new NotFoundException("Không tìm thấy Nhóm Topping này.");
            }

            return toppingGroup;
        }
    }
}
