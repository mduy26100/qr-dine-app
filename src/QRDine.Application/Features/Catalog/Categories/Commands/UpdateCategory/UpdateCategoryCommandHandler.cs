using QRDine.Application.Common.Abstractions.Identity;
using QRDine.Application.Common.Abstractions.Persistence;
using QRDine.Application.Common.Exceptions;
using QRDine.Application.Features.Catalog.Categories.DTOs;
using QRDine.Application.Features.Catalog.Categories.Specifications;
using QRDine.Application.Features.Catalog.Repositories;

namespace QRDine.Application.Features.Catalog.Categories.Commands.UpdateCategory
{
    public class UpdateCategoryCommandHandler : IRequestHandler<UpdateCategoryCommand, CategoryResponseDto>
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IMapper _mapper;
        private readonly ICurrentUserService _currentUserService;
        private readonly IApplicationDbContext _context;

        public UpdateCategoryCommandHandler(
            ICategoryRepository categoryRepository,
            IMapper mapper,
            ICurrentUserService currentUserService,
            IApplicationDbContext context)
        {
            _categoryRepository = categoryRepository;
            _mapper = mapper;
            _currentUserService = currentUserService;
            _context = context;
        }

        public async Task<CategoryResponseDto> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
        {
            var merchantId = _currentUserService.MerchantId
                ?? throw new UnauthorizedAccessException("Merchant context is missing.");

            var existingCategory = await _categoryRepository.GetByIdAsync(request.Id, cancellationToken)
                ?? throw new NotFoundException($"Category with ID {request.Id} not found.");

            if (request.Dto.ParentId.HasValue)
            {
                var targetParent = await _categoryRepository.GetByIdAsync(request.Dto.ParentId.Value, cancellationToken)
                    ?? throw new NotFoundException("Target parent category does not exist.");

                if (targetParent.Name.Equals(request.Dto.Name, StringComparison.OrdinalIgnoreCase))
                    throw new BusinessRuleException("A child category cannot have the exact same name as its parent.");

                if (request.Dto.ParentId != existingCategory.ParentId)
                {
                    if (request.Dto.ParentId.Value == request.Id)
                        throw new BusinessRuleException("A category cannot be its own parent.");

                    var hasChildrenSpec = new CategoryHasChildrenSpec(request.Id);
                    if (await _categoryRepository.AnyAsync(hasChildrenSpec, cancellationToken))
                        throw new BusinessRuleException("This category already has sub-categories. It cannot become a child of another category.");

                    if (targetParent.ParentId.HasValue)
                        throw new BusinessRuleException("Cannot assign a parent that is already a child. Only 1 level of hierarchy allowed.");
                }
            }

            var conflictSpec = new CategoryByNameSpec(merchantId, request.Dto.Name, request.Id);
            if (await _categoryRepository.AnyAsync(conflictSpec, cancellationToken))
            {
                throw new ConflictException($"Category name '{request.Dto.Name}' already exists in your menu.");
            }

            bool isMovingToNewGroup = existingCategory.ParentId != request.Dto.ParentId;
            bool isChangingOrderExplicitly = request.Dto.DisplayOrder > 0 && request.Dto.DisplayOrder != existingCategory.DisplayOrder;
            bool requiresOrderRecalculation = isMovingToNewGroup || isChangingOrderExplicitly;

            await using var transaction = await _context.BeginTransactionAsync(cancellationToken);

            try
            {
                existingCategory.Name = request.Dto.Name;
                existingCategory.Description = request.Dto.Description;
                existingCategory.IsActive = request.Dto.IsActive;
                existingCategory.ParentId = request.Dto.ParentId;

                if (requiresOrderRecalculation)
                {
                    if (request.Dto.DisplayOrder > 0)
                    {
                        await _categoryRepository.ShiftDisplayOrdersAsync(request.Dto.ParentId, request.Dto.DisplayOrder, cancellationToken);
                        existingCategory.DisplayOrder = request.Dto.DisplayOrder;
                    }
                    else
                    {
                        var maxOrder = await _categoryRepository.GetMaxDisplayOrderAsync(request.Dto.ParentId, cancellationToken);
                        existingCategory.DisplayOrder = maxOrder + 1;
                    }
                }

                await _categoryRepository.UpdateAsync(existingCategory, cancellationToken);
                await transaction.CommitAsync(cancellationToken);
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }

            return _mapper.Map<CategoryResponseDto>(existingCategory);
        }
    }
}
