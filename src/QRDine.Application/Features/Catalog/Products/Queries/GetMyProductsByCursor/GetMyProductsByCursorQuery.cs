using QRDine.Application.Common.Models;
using QRDine.Application.Features.Catalog.Products.DTOs;

namespace QRDine.Application.Features.Catalog.Products.Queries.GetMyProductsByCursor
{
    public class GetMyProductsByCursorQuery : IRequest<CursorPagedResult<ProductDto>>
    {
        public string? SearchTerm { get; set; }
        public Guid? CategoryId { get; set; }
        public bool? IsAvailable { get; set; }

        public DateTime? CursorCreatedAt { get; set; }
        public Guid? CursorId { get; set; }

        private const int MaxLimit = 50;
        private int _limit = 10;
        public int Limit
        {
            get => _limit;
            set => _limit = (value > MaxLimit) ? MaxLimit : value;
        }
    }
}
