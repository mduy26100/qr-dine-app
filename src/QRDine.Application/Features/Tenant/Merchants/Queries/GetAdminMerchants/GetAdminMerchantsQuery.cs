using QRDine.Application.Common.Models;
using QRDine.Application.Features.Tenant.Merchants.DTOs;

namespace QRDine.Application.Features.Tenant.Merchants.Queries.GetAdminMerchants
{
    public class GetAdminMerchantsQuery : PaginationRequest, IRequest<PagedResult<AdminMerchantDto>>
    {
        public string? SearchKeyword { get; set; }
    }
}
