using QRDine.Application.Common.Models;
using QRDine.Application.Features.Tenant.Merchants.DTOs;

namespace QRDine.Application.Features.Tenant.Merchants.Queries.GetAdminMerchants
{
    public class GetAdminMerchantsQuery : IRequest<PagedResult<AdminMerchantDto>>
    {
        public string? SearchKeyword { get; set; }

        private int _pageIndex = 1;
        public int PageIndex
        {
            get => _pageIndex;
            set => _pageIndex = value < 1 ? 1 : value;
        }

        private const int MaxPageSize = 50;
        private int _pageSize = 10;
        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = value > MaxPageSize ? MaxPageSize : (value < 1 ? 1 : value);
        }
    }
}
