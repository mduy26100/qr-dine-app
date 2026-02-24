using QRDine.Application.Features.Catalog.Categories.DTOs;

namespace QRDine.Application.Features.Catalog.Categories.Commands.CreateCategory
{
    public record CreateCategoryCommand(CreateCategoryDto Dto) : IRequest<CategoryResponseDto>;
}
