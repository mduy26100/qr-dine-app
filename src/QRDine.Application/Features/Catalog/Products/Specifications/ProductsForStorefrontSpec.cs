using QRDine.Application.Features.Catalog.Products.DTOs;
using QRDine.Domain.Catalog;

namespace QRDine.Application.Features.Catalog.Products.Specifications
{
    public class ProductsForStorefrontSpec : Specification<Product, ProductForStorefrontDto>
    {
        public ProductsForStorefrontSpec(Guid merchantId, Guid categoryId)
        {
            Query.Where(x => x.MerchantId == merchantId && x.CategoryId == categoryId);

            Query.OrderByDescending(x => x.IsAvailable)
                 .ThenBy(x => x.Name);

            Query.Select(x => new ProductForStorefrontDto
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description,
                ImageUrl = x.ImageUrl,
                Price = x.Price,
                IsAvailable = x.IsAvailable
            });
        }
    }
}
