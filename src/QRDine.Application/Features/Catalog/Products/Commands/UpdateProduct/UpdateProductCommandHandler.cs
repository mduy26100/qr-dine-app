using QRDine.Application.Common.Abstractions.ExternalServices.FileUpload;
using QRDine.Application.Common.Exceptions;
using QRDine.Application.Features.Catalog.Products.DTOs;
using QRDine.Application.Features.Catalog.Products.Specifications;
using QRDine.Application.Features.Catalog.Repositories;

namespace QRDine.Application.Features.Catalog.Products.Commands.UpdateProduct
{
    public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, ProductResponseDto>
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IFileUploadService _fileUploadService;
        private readonly IMapper _mapper;

        public UpdateProductCommandHandler(
            IProductRepository productRepository,
            ICategoryRepository categoryRepository,
            IFileUploadService fileUploadService,
            IMapper mapper)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _fileUploadService = fileUploadService;
            _mapper = mapper;
        }

        public async Task<ProductResponseDto> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
        {
            var existingProduct = await _productRepository.GetByIdAsync(request.Id, cancellationToken)
                ?? throw new NotFoundException($"Product with ID {request.Id} not found.");

            if (existingProduct.CategoryId != request.Dto.CategoryId)
            {
                var category = await _categoryRepository.GetByIdAsync(request.Dto.CategoryId, cancellationToken)
                    ?? throw new NotFoundException($"Category with ID {request.Dto.CategoryId} not found.");

                if (category.ParentId == null)
                {
                    throw new BusinessRuleException("Products can only be assigned to sub-categories (child categories).");
                }
            }

            if (existingProduct.Name != request.Dto.Name || existingProduct.CategoryId != request.Dto.CategoryId)
            {
                var conflictSpec = new ProductNameConflictSpec(request.Dto.CategoryId, request.Dto.Name, request.Id);
                if (await _productRepository.AnyAsync(conflictSpec, cancellationToken))
                {
                    throw new ConflictException($"A product with the name '{request.Dto.Name}' already exists in this category.");
                }
            }

            if (request.Dto.ImgContent != null && !string.IsNullOrWhiteSpace(request.Dto.ImgFileName))
            {
                var uploadRequest = new FileUploadRequest
                {
                    Content = request.Dto.ImgContent,
                    FileName = request.Dto.ImgFileName,
                    ContentType = request.Dto.ImgContentType ?? "image/jpeg"
                };

                existingProduct.ImageUrl = await _fileUploadService.UploadAsync(uploadRequest, cancellationToken);
            }

            existingProduct.Name = request.Dto.Name;
            existingProduct.Description = request.Dto.Description;
            existingProduct.Price = request.Dto.Price;
            existingProduct.IsAvailable = request.Dto.IsAvailable;
            existingProduct.CategoryId = request.Dto.CategoryId;

            await _productRepository.UpdateAsync(existingProduct, cancellationToken);

            return _mapper.Map<ProductResponseDto>(existingProduct);
        }
    }
}
