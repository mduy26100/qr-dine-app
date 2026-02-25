using QRDine.Application.Features.Catalog.Products.DTOs;

namespace QRDine.Application.Features.Catalog.Products.Commands.UpdateProduct
{
    public record UpdateProductCommand(Guid Id, UpdateProductDto Dto) : IRequest<ProductResponseDto>;
}
