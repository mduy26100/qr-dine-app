using QRDine.Application.Features.Catalog.Products.DTOs;
using QRDine.Domain.Catalog;

namespace QRDine.Application.Features.Catalog.Products.Extensions
{
    public static class ProductExtensions
    {
        public static Expression<Func<Product, ProductPriceDto>> ToProductPriceDto =>
            p => new ProductPriceDto
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price
            };
    }
}
