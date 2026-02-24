using QRDine.Application.Features.Catalog.Categories.DTOs;
using QRDine.Domain.Catalog;

namespace QRDine.Application.Features.Catalog.Mappings
{
    public class CategoryMappingProfile : Profile
    {
        public CategoryMappingProfile()
        {
            CreateMap<Category, CreateCategoryDto>().ReverseMap();
            CreateMap<Category, CategoryResponseDto>().ReverseMap();
            CreateMap<Category, CategoryTreeDto>().ReverseMap();
        }
    }
}
