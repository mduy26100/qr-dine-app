namespace QRDine.Application.Tests.Features.Catalog.Products.Commands.CreateProduct
{
    public class CreateProductCommandHandlerTests
    {
        private readonly Mock<IProductRepository> _productRepo;
        private readonly Mock<ICategoryRepository> _categoryRepo;
        private readonly Mock<IFileUploadService> _fileUploadService;
        private readonly Mock<IMapper> _mapper;
        private readonly CreateProductCommandHandler _handler;
        private readonly CatalogFixture _fixture;

        public CreateProductCommandHandlerTests()
        {
            _fixture = new CatalogFixture();
            _productRepo = CatalogRepositoryMocks.CreateProductRepositoryMock();
            _categoryRepo = CatalogRepositoryMocks.CreateCategoryRepositoryMock();
            _fileUploadService = CatalogServiceMocks.CreateFileUploadServiceMock();
            _mapper = CatalogServiceMocks.CreateMapperMock();

            _handler = new CreateProductCommandHandler(
                _productRepo.Object,
                _categoryRepo.Object,
                _fileUploadService.Object,
                _mapper.Object
            );
        }

        [Fact]
        public async Task Handle_CategoryNotExists_ShouldThrowNotFoundException()
        {
            var createDto = new CreateProductDtoBuilder()
                .WithCategoryId(_fixture.CategoryId)
                .Build();

            var command = new CreateProductCommand(createDto);
            var cancellationToken = CancellationToken.None;

            _categoryRepo
                .Setup(x => x.GetByIdAsync(_fixture.CategoryId, cancellationToken))
                .ReturnsAsync((Category?)null);

            await Assert.ThrowsAsync<NotFoundException>(
                () => _handler.Handle(command, cancellationToken)
            );
        }

        [Fact]
        public async Task Handle_CategoryIsMainCategory_ShouldThrowBusinessRuleException()
        {
            var createDto = new CreateProductDtoBuilder()
                .WithCategoryId(_fixture.CategoryId)
                .Build();

            var command = new CreateProductCommand(createDto);
            var cancellationToken = CancellationToken.None;

            var mainCategory = new CategoryBuilder()
                .WithId(_fixture.CategoryId)
                .WithParentId(null)
                .Build();

            _categoryRepo
                .Setup(x => x.GetByIdAsync(_fixture.CategoryId, cancellationToken))
                .ReturnsAsync(mainCategory);

            await Assert.ThrowsAsync<BusinessRuleException>(
                () => _handler.Handle(command, cancellationToken)
            );
        }

        [Fact]
        public async Task Handle_ValidRequest_ShouldCreateProductSuccessfully()
        {
            var createDto = new CreateProductDtoBuilder()
                .WithName("Laptop")
                .WithCategoryId(_fixture.CategoryId)
                .Build();

            var command = new CreateProductCommand(createDto);
            var cancellationToken = CancellationToken.None;

            var subCategory = new CategoryBuilder()
                .WithId(_fixture.CategoryId)
                .WithParentId(_fixture.ParentCategoryId)
                .Build();

            var createdProduct = new ProductBuilder()
                .WithId(_fixture.ProductId)
                .WithName("Laptop")
                .WithCategoryId(_fixture.CategoryId)
                .Build();

            var responseDto = new ProductResponseDto
            {
                Id = _fixture.ProductId,
                Name = "Laptop",
                Price = 1000m,
                IsAvailable = true
            };

            _categoryRepo
                .Setup(x => x.GetByIdAsync(_fixture.CategoryId, cancellationToken))
                .ReturnsAsync(subCategory);

            _productRepo
                .Setup(x => x.FirstOrDefaultAsync(It.IsAny<ISingleResultSpecification<Product>>(), cancellationToken))
                .ReturnsAsync((Product?)null);

            _mapper
                .Setup(x => x.Map<Product>(createDto))
                .Returns(createdProduct);

            _mapper
                .Setup(x => x.Map<ProductResponseDto>(createdProduct))
                .Returns(responseDto);

            _productRepo
                .Setup(x => x.AddAsync(createdProduct, cancellationToken))
                .ReturnsAsync((Product p, CancellationToken ct) => p);

            var result = await _handler.Handle(command, cancellationToken);

            result.Should().NotBeNull();
            result.Id.Should().Be(_fixture.ProductId);
            result.Name.Should().Be("Laptop");
            _productRepo.Verify(x => x.AddAsync(createdProduct, cancellationToken), Times.Once);
        }

        [Fact]
        public async Task Handle_DuplicateProductName_ShouldThrowConflictException()
        {
            var command = new CreateProductCommand(
                new CreateProductDtoBuilder()
                    .WithName("Chicken")
                    .WithCategoryId(_fixture.CategoryId)
                    .Build()
            );

            var subCategory = new CategoryBuilder()
                .WithId(_fixture.CategoryId)
                .WithParentId(_fixture.ParentCategoryId)
                .Build();

            var existingProduct = new ProductBuilder()
                .WithName("Chicken")
                .WithCategoryId(_fixture.CategoryId)
                .WithIsDeleted(false)
                .Build();

            _categoryRepo
                .Setup(x => x.GetByIdAsync(_fixture.CategoryId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(subCategory);

            _productRepo
                .Setup(x => x.FirstOrDefaultAsync(It.IsAny<ISpecification<Product>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingProduct);

            await Assert.ThrowsAsync<ConflictException>(
                () => _handler.Handle(command, CancellationToken.None)
            );
        }

        [Fact]
        public async Task Handle_DeletedProductWithSameName_ShouldRestoreProduct()
        {
            var command = new CreateProductCommand(
                new CreateProductDtoBuilder()
                    .WithName("Laptop")
                    .WithCategoryId(_fixture.CategoryId)
                    .WithPrice(1000m)
                    .Build()
            );

            var subCategory = new CategoryBuilder()
                .WithId(_fixture.CategoryId)
                .WithParentId(_fixture.ParentCategoryId)
                .Build();

            var deletedProduct = new ProductBuilder()
                .WithName("Laptop")
                .WithCategoryId(_fixture.CategoryId)
                .WithIsDeleted(true)
                .Build();

            var responseDto = new ProductResponseDto
            {
                Id = deletedProduct.Id,
                Name = "Laptop",
                Price = 1000m
            };

            _categoryRepo
                .Setup(x => x.GetByIdAsync(_fixture.CategoryId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(subCategory);

            _productRepo
                .Setup(x => x.FirstOrDefaultAsync(It.IsAny<ISpecification<Product>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(deletedProduct);

            _productRepo
                .Setup(x => x.UpdateAsync(deletedProduct, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _mapper
                .Setup(x => x.Map<ProductResponseDto>(deletedProduct))
                .Returns(responseDto);

            var result = await _handler.Handle(command, CancellationToken.None);

            deletedProduct.IsDeleted.Should().BeFalse();
            result.Should().NotBeNull();
            _productRepo.Verify(x => x.UpdateAsync(deletedProduct, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_WithValidImageFile_ShouldUploadAndSetImageUrl()
        {
            var imageStream = new MemoryStream(new byte[] { 1, 2, 3 });
            var createDto = new CreateProductDtoBuilder()
                .WithName("Laptop")
                .WithCategoryId(_fixture.CategoryId)
                .WithImage(imageStream, "laptop.jpg", "image/jpeg")
                .Build();

            var command = new CreateProductCommand(createDto);
            var cancellationToken = CancellationToken.None;

            var subCategory = new CategoryBuilder()
                .WithId(_fixture.CategoryId)
                .WithParentId(_fixture.ParentCategoryId)
                .Build();

            var createdProduct = new ProductBuilder()
                .WithName("Laptop")
                .WithCategoryId(_fixture.CategoryId)
                .WithImageUrl("https://example.com/laptop.jpg")
                .Build();

            var responseDto = new ProductResponseDto
            {
                Id = createdProduct.Id,
                Name = "Laptop",
                ImageUrl = "https://example.com/laptop.jpg"
            };

            _categoryRepo
                .Setup(x => x.GetByIdAsync(_fixture.CategoryId, cancellationToken))
                .ReturnsAsync(subCategory);

            _productRepo
                .Setup(x => x.FirstOrDefaultAsync(It.IsAny<ISingleResultSpecification<Product>>(), cancellationToken))
                .ReturnsAsync((Product?)null);

            _fileUploadService
                .Setup(x => x.UploadAsync(It.IsAny<FileUploadRequest>(), cancellationToken))
                .ReturnsAsync("https://example.com/laptop.jpg");

            _mapper
                .Setup(x => x.Map<Product>(createDto))
                .Returns(createdProduct);

            _mapper
                .Setup(x => x.Map<ProductResponseDto>(createdProduct))
                .Returns(responseDto);

            _productRepo
                .Setup(x => x.AddAsync(createdProduct, cancellationToken))
                .ReturnsAsync((Product p, CancellationToken ct) => p);

            var result = await _handler.Handle(command, cancellationToken);

            result.ImageUrl.Should().Be("https://example.com/laptop.jpg");
            _fileUploadService.Verify(
                x => x.UploadAsync(It.IsAny<FileUploadRequest>(), cancellationToken),
                Times.Once
            );
        }
    }
}
