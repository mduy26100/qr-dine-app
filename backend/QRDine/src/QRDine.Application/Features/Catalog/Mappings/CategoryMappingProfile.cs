using QRDine.Application.Features.Catalog.Categories.DTOs;
using QRDine.Domain.Catalog;

namespace QRDine.Application.Features.Catalog.Mappings
{
    public class CategoryMappingProfile : Profile
    {
        public CategoryMappingProfile()
        {
            CreateMap<CreateCategoryDto, Category>();
            CreateMap<Category, CategoryResponseDto>();
        }
    }
}
