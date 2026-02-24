using QRDine.Application.Features.Catalog.Categories.DTOs;

namespace QRDine.Application.Features.Catalog.Categories.Commands.UpdateCategory
{
    public record UpdateCategoryCommand(Guid Id, UpdateCategoryDto Dto) : IRequest<CategoryResponseDto>;
}
