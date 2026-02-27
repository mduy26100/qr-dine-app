using QRDine.Application.Features.Catalog.Tables.DTOs;

namespace QRDine.Application.Features.Catalog.Tables.Queries.GetMyTables
{
    public record GetMyTablesQuery(bool? IsOccupied) : IRequest<List<TableResponseDto>>;
}
