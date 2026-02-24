using QRDine.Application.Common.Abstractions.Persistence;
using QRDine.Application.Common.Exceptions;
using QRDine.Application.Features.Catalog.Categories.DTOs;
using QRDine.Application.Features.Catalog.Categories.Specifications;
using QRDine.Application.Features.Catalog.Repositories;
using QRDine.Domain.Catalog;

namespace QRDine.Application.Features.Catalog.Categories.Commands.CreateCategory
{
    public class CreateCategoryCommandHandler : IRequestHandler<CreateCategoryCommand, CategoryResponseDto>
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IMapper _mapper;
        private readonly IApplicationDbContext _context;

        public CreateCategoryCommandHandler(
            ICategoryRepository categoryRepository,
            IMapper mapper,
            IApplicationDbContext context)
        {
            _categoryRepository = categoryRepository;
            _mapper = mapper;
            _context = context;
        }

        public async Task<CategoryResponseDto> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
        {
            if (request.Dto.ParentId.HasValue)
            {
                var parentCategory = await _categoryRepository.GetByIdAsync(request.Dto.ParentId.Value, cancellationToken)
                    ?? throw new NotFoundException("Parent category does not exist.");

                if (parentCategory.ParentId.HasValue)
                {
                    throw new BusinessRuleException("Cannot assign a parent that is already a child. Only 1 level of hierarchy allowed.");
                }
            }

            var spec = new CategoryByNameSpec(request.Dto.Name);
            if (await _categoryRepository.AnyAsync(spec, cancellationToken))
            {
                throw new ConflictException($"Category name '{request.Dto.Name}' already exists at this level.");
            }

            var category = _mapper.Map<Category>(request.Dto);

            await using var transaction = await _context.BeginTransactionAsync(cancellationToken);

            try
            {
                if (request.Dto.DisplayOrder > 0)
                {
                    await _categoryRepository.ShiftDisplayOrdersAsync(request.Dto.ParentId, request.Dto.DisplayOrder, cancellationToken);
                }
                else
                {
                    var maxOrder = await _categoryRepository.GetMaxDisplayOrderAsync(request.Dto.ParentId, cancellationToken);
                    category.DisplayOrder = maxOrder + 1;
                }

                await _categoryRepository.AddAsync(category, cancellationToken);

                await transaction.CommitAsync(cancellationToken);
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }

            return _mapper.Map<CategoryResponseDto>(category);
        }
    }
}
