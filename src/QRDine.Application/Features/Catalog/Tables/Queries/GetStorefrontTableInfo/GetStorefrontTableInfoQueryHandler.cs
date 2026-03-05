using QRDine.Application.Common.Exceptions;
using QRDine.Application.Features.Catalog.Repositories;
using QRDine.Application.Features.Catalog.Tables.DTOs;
using QRDine.Application.Features.Catalog.Tables.Specifications;

namespace QRDine.Application.Features.Catalog.Tables.Queries.GetStorefrontTableInfo
{
    public class GetStorefrontTableInfoQueryHandler : IRequestHandler<GetStorefrontTableInfoQuery, StorefrontTableInfoDto>
    {
        private readonly ITableRepository _tableRepository;

        public GetStorefrontTableInfoQueryHandler(ITableRepository tableRepository)
        {
            _tableRepository = tableRepository;
        }

        public async Task<StorefrontTableInfoDto> Handle(GetStorefrontTableInfoQuery request, CancellationToken cancellationToken)
        {
            var spec = new TableByTokenSpec(request.MerchantId, request.QrCodeToken);
            var table = await _tableRepository.SingleOrDefaultAsync(spec, cancellationToken);

            if (table == null)
                throw new NotFoundException("Mã QR không hợp lệ hoặc bàn không tồn tại.");

            var sessionId = table.IsOccupied && table.CurrentSessionId.HasValue
                                ? table.CurrentSessionId.Value
                                : Guid.NewGuid();

            return new StorefrontTableInfoDto
            {
                TableId = table.Id,
                MerchantId = table.MerchantId,
                TableName = table.Name,
                IsOccupied = table.IsOccupied,
                SessionId = sessionId
            };
        }
    }
}
