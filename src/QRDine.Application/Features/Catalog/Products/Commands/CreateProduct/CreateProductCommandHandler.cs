using QRDine.Application.Common.Abstractions.ExternalServices.FileUpload;
using QRDine.Application.Common.Exceptions;
using QRDine.Application.Features.Catalog.Products.DTOs;
using QRDine.Application.Features.Catalog.Products.Specifications;
using QRDine.Application.Features.Catalog.Repositories;
using QRDine.Domain.Catalog;

namespace QRDine.Application.Features.Catalog.Products.Commands.CreateProduct
{
    public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, ProductResponseDto>
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IFileUploadService _fileUploadService;
        private readonly IMapper _mapper;

        public CreateProductCommandHandler(
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

        public async Task<ProductResponseDto> Handle(CreateProductCommand request, CancellationToken cancellationToken)
        {
            var category = await _categoryRepository.GetByIdAsync(request.Dto.CategoryId, cancellationToken)
                ?? throw new NotFoundException($"Category with ID {request.Dto.CategoryId} not found.");

            if (category.ParentId == null)
            {
                throw new BusinessRuleException("Products can only be assigned to sub-categories (child categories).");
            }

            var conflictSpec = new ProductNameConflictSpec(request.Dto.CategoryId, request.Dto.Name, includeDeleted: true);
            var existingProduct = await _productRepository.FirstOrDefaultAsync(conflictSpec, cancellationToken);

            string imgUrl = string.Empty;
            if (request.Dto.ImgContent != null && !string.IsNullOrWhiteSpace(request.Dto.ImgFileName))
            {
                var uploadRequest = new FileUploadRequest
                {
                    Content = request.Dto.ImgContent,
                    FileName = request.Dto.ImgFileName,
                    ContentType = request.Dto.ImgContentType ?? "image/jpeg"
                };
                imgUrl = await _fileUploadService.UploadAsync(uploadRequest, cancellationToken);
            }

            if (existingProduct != null)
            {
                if (!existingProduct.IsDeleted)
                {
                    throw new ConflictException($"A product with the name '{request.Dto.Name}' already exists in this category.");
                }

                existingProduct.IsDeleted = false;
                existingProduct.Description = request.Dto.Description;
                existingProduct.Price = request.Dto.Price;
                existingProduct.IsAvailable = request.Dto.IsAvailable;
                existingProduct.ImageUrl = string.IsNullOrWhiteSpace(imgUrl) ? null : imgUrl;

                await _productRepository.UpdateAsync(existingProduct, cancellationToken);

                return _mapper.Map<ProductResponseDto>(existingProduct);
            }

            var product = _mapper.Map<Product>(request.Dto);
            product.ImageUrl = string.IsNullOrWhiteSpace(imgUrl) ? null : imgUrl;

            await _productRepository.AddAsync(product, cancellationToken);

            return _mapper.Map<ProductResponseDto>(product);
        }
    }
}