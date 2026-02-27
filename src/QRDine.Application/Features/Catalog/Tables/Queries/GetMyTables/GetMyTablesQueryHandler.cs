using QRDine.Application.Features.Catalog.Repositories;
using QRDine.Application.Features.Catalog.Tables.DTOs;
using QRDine.Application.Features.Catalog.Tables.Specifications;

namespace QRDine.Application.Features.Catalog.Tables.Queries.GetMyTables
{
    public class GetMyTablesQueryHandler : IRequestHandler<GetMyTablesQuery, List<TableResponseDto>>
    {
        private readonly ITableRepository _tableRepository;

        public GetMyTablesQueryHandler(ITableRepository tableRepository)
        {
            _tableRepository = tableRepository;
        }

        public async Task<List<TableResponseDto>> Handle(GetMyTablesQuery request, CancellationToken cancellationToken)
        {
            var spec = new TablesFilterSpec(request.IsOccupied);

            var tables = await _tableRepository.ListAsync(spec, cancellationToken);

            return tables;
        }
    }
}
