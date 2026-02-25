namespace QRDine.Application.Features.Catalog.Products.Commands.DeleteProduct
{
    public record DeleteProductCommand(Guid Id) : IRequest;
}
