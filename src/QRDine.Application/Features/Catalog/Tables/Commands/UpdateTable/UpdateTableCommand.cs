using QRDine.Application.Features.Catalog.Tables.DTOs;

namespace QRDine.Application.Features.Catalog.Tables.Commands.UpdateTable
{
    public record UpdateTableCommand(Guid Id, UpdateTableDto Dto) : IRequest<TableResponseDto>;
}
