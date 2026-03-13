using QRDine.Application.Common.Models;
using QRDine.Application.Features.Tenant.Merchants.DTOs;
using QRDine.Application.Features.Tenant.Merchants.Specifications;
using QRDine.Application.Features.Tenant.Repositories;

namespace QRDine.Application.Features.Tenant.Merchants.Queries.GetAdminMerchants
{
    public class GetAdminMerchantsQueryHandler : IRequestHandler<GetAdminMerchantsQuery, PagedResult<AdminMerchantDto>>
    {
        private readonly IMerchantRepository _merchantRepository;

        public GetAdminMerchantsQueryHandler(IMerchantRepository merchantRepository)
        {
            _merchantRepository = merchantRepository;
        }

        public async Task<PagedResult<AdminMerchantDto>> Handle(GetAdminMerchantsQuery request, CancellationToken cancellationToken)
        {
            var spec = new MerchantsWithSubscriptionSpec(request.SearchKeyword, request.PageNumber, request.PageSize);
            var items = await _merchantRepository.ListAsync(spec, cancellationToken);

            var countSpec = new MerchantsCountSpec(request.SearchKeyword);
            var totalCount = await _merchantRepository.CountAsync(countSpec, cancellationToken);

            return new PagedResult<AdminMerchantDto>(
                items,
                totalCount,
                request.PageNumber,
                request.PageSize);
        }
    }
}
