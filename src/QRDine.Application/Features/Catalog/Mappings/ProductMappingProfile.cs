using QRDine.Application.Features.Catalog.Products.DTOs;
using QRDine.Domain.Catalog;

namespace QRDine.Application.Features.Catalog.Mappings
{
    public class ProductMappingProfile : Profile
    {
        public ProductMappingProfile()
        {
            CreateMap<CreateProductDto, Product>()
                .ForMember(dest => dest.ProductToppingGroups, opt => opt.Ignore())
                .ForMember(dest => dest.ImageUrl, opt => opt.Ignore())
                .ReverseMap();

            CreateMap<Product, ProductResponseDto>().ReverseMap();
        }
    }
}
