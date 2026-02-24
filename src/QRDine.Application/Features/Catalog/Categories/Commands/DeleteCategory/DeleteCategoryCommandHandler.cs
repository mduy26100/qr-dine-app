using QRDine.Application.Common.Abstractions.Identity;
using QRDine.Application.Common.Exceptions;
using QRDine.Application.Features.Catalog.Categories.Specifications;
using QRDine.Application.Features.Catalog.Repositories;

namespace QRDine.Application.Features.Catalog.Categories.Commands.DeleteCategory
{
    public class DeleteCategoryCommandHandler : IRequestHandler<DeleteCategoryCommand>
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly ICurrentUserService _currentUserService;

        public DeleteCategoryCommandHandler(
            ICategoryRepository categoryRepository,
            ICurrentUserService currentUserService)
        {
            _categoryRepository = categoryRepository;
            _currentUserService = currentUserService;
        }

        public async Task Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
        {
            var merchantId = _currentUserService.MerchantId
                ?? throw new UnauthorizedAccessException("Merchant context is missing.");

            var existingCategory = await _categoryRepository.GetByIdAsync(request.Id, cancellationToken)
                ?? throw new NotFoundException($"Category with ID {request.Id} not found.");

            if (existingCategory.MerchantId != merchantId)
                throw new ForbiddenException("You do not have permission to delete this category.");

            var hasChildrenSpec = new CategoryHasChildrenSpec(request.Id);
            if (await _categoryRepository.AnyAsync(hasChildrenSpec, cancellationToken))
            {
                throw new BusinessRuleException("Cannot delete this category because it contains sub-categories. Please delete or reassign them first.");
            }

            var hasProductsSpec = new CategoryHasProductsSpec(request.Id);
            if (await _categoryRepository.AnyAsync(hasProductsSpec, cancellationToken))
            {
                throw new BusinessRuleException("Cannot delete this category because it contains active products. Please remove the products first.");
            }

            await _categoryRepository.DeleteAsync(existingCategory, cancellationToken);
        }
    }
}
