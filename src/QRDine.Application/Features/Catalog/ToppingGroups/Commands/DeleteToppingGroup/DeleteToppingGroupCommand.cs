namespace QRDine.Application.Features.Catalog.ToppingGroups.Commands.DeleteToppingGroup
{
    public record DeleteToppingGroupCommand(Guid Id) : IRequest<Unit>;
}
