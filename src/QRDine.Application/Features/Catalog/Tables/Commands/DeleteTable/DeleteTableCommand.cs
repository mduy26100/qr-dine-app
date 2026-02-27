namespace QRDine.Application.Features.Catalog.Tables.Commands.DeleteTable
{
    public record DeleteTableCommand(Guid Id) : IRequest;
}
