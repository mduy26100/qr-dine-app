using QRDine.Application.Features.Catalog.Products.DTOs;

namespace QRDine.Application.Features.Catalog.Products.Commands.CreateProduct
{
    public record CreateProductCommand(CreateProductDto Dto) : IRequest<ProductResponseDto>;
}
