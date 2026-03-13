using QRDine.Application.Common.Models;
using QRDine.Application.Features.Sales.Orders.DTOs;

namespace QRDine.Application.Features.Sales.Orders.Queries.GetOrderHistory
{
    public class GetOrderHistoryQuery : PaginationRequest, IRequest<PagedResult<OrderListDto>>
    {
        public string? SearchTerm { get; set; }

        private const int MaxPageSize = 50;

        public new int PageSize
        {
            get => base.PageSize;
            set => base.PageSize = value > MaxPageSize ? MaxPageSize : (value < 1 ? 1 : value);
        }

        public new int PageNumber
        {
            get => base.PageNumber;
            set => base.PageNumber = value < 1 ? 1 : value;
        }
    }
}
