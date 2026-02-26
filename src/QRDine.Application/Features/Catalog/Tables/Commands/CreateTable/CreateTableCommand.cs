using QRDine.Application.Features.Catalog.Tables.DTOs;

namespace QRDine.Application.Features.Catalog.Tables.Commands.CreateTable
{
    public record CreateTableCommand(string Name) : IRequest<TableResponseDto>;
}
