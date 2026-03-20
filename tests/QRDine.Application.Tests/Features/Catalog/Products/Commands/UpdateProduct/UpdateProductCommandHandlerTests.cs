namespace QRDine.Application.Tests.Features.Catalog.Products.Commands.UpdateProduct
{
    public class UpdateProductCommandHandlerTests
    {
        private readonly Mock<IProductRepository> _productRepo;
        private readonly Mock<ICategoryRepository> _categoryRepo;
        private readonly Mock<IFileUploadService> _fileUploadService;
        private readonly Mock<IMapper> _mapper;
        private readonly UpdateProductCommandHandler _handler;
        private readonly CatalogFixture _fixture;

        public UpdateProductCommandHandlerTests()
        {
            _fixture = new CatalogFixture();

            _productRepo = CatalogRepositoryMocks.CreateProductRepositoryMock();
            _categoryRepo = CatalogRepositoryMocks.CreateCategoryRepositoryMock();
            _fileUploadService = CatalogServiceMocks.CreateFileUploadServiceMock();
            _mapper = CatalogServiceMocks.CreateMapperMock();

            _handler = new UpdateProductCommandHandler(
                _productRepo.Object,
                _categoryRepo.Object,
                _fileUploadService.Object,
                _mapper.Object
            );
        }

        [Fact]
        public async Task Handle_ProductNotExists_ShouldThrowNotFoundException()
        {
            var updateDto = new UpdateProductDtoBuilder().Build();
            var command = new UpdateProductCommand(_fixture.ProductId, updateDto);

            _productRepo
                .Setup(x => x.GetByIdAsync(_fixture.ProductId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Product?)null);

            await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_ChangingCategoryToNonExistent_ShouldThrowNotFoundException()
        {
            var newCategoryId = Guid.NewGuid();
            var updateDto = new UpdateProductDtoBuilder()
                .WithCategoryId(newCategoryId)
                .Build();

            var command = new UpdateProductCommand(_fixture.ProductId, updateDto);

            var existingProduct = new ProductBuilder()
                .WithId(_fixture.ProductId)
                .WithCategoryId(_fixture.CategoryId)
                .Build();

            _productRepo
                .Setup(x => x.GetByIdAsync(_fixture.ProductId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingProduct);

            _categoryRepo
                .Setup(x => x.GetByIdAsync(newCategoryId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Category?)null);

            await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_AssigningToMainCategory_ShouldThrowBusinessRuleException()
        {
            var updateDto = new UpdateProductDtoBuilder()
                .WithCategoryId(_fixture.CategoryId)
                .Build();

            var command = new UpdateProductCommand(_fixture.ProductId, updateDto);

            var existingProduct = new ProductBuilder()
                .WithId(_fixture.ProductId)
                .WithCategoryId(Guid.NewGuid())
                .Build();

            var mainCategory = new CategoryBuilder()
                .WithId(_fixture.CategoryId)
                .WithParentId(null)
                .Build();

            _productRepo
                .Setup(x => x.GetByIdAsync(_fixture.ProductId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingProduct);

            _categoryRepo
                .Setup(x => x.GetByIdAsync(_fixture.CategoryId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(mainCategory);

            await Assert.ThrowsAsync<BusinessRuleException>(() => _handler.Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_ValidRequest_ShouldUpdateProductSuccessfully()
        {
            var updateDto = new UpdateProductDtoBuilder()
                .WithName("Updated Laptop")
                .WithPrice(1200m)
                .WithCategoryId(_fixture.CategoryId)
                .Build();

            var command = new UpdateProductCommand(_fixture.ProductId, updateDto);

            var existingProduct = new ProductBuilder()
                .WithId(_fixture.ProductId)
                .WithCategoryId(_fixture.CategoryId)
                .Build();

            var subCategory = new CategoryBuilder()
                .WithId(_fixture.CategoryId)
                .WithParentId(Guid.NewGuid())
                .Build();

            var responseDto = new ProductResponseDto { Id = _fixture.ProductId, Name = "Updated Laptop" };

            _productRepo.Setup(x => x.GetByIdAsync(_fixture.ProductId, It.IsAny<CancellationToken>())).ReturnsAsync(existingProduct);
            _categoryRepo.Setup(x => x.GetByIdAsync(_fixture.CategoryId, It.IsAny<CancellationToken>())).ReturnsAsync(subCategory);

            _productRepo.Setup(x => x.AnyAsync(It.IsAny<ISpecification<Product>>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);

            _mapper.Setup(x => x.Map<ProductResponseDto>(existingProduct)).Returns(responseDto);

            var result = await _handler.Handle(command, CancellationToken.None);

            result.Should().NotBeNull();
            _productRepo.Verify(x => x.UpdateAsync(existingProduct, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_DuplicateProductNameInCategory_ShouldThrowConflictException()
        {
            var updateDto = new UpdateProductDtoBuilder()
                .WithName("Duplicate Name")
                .WithCategoryId(_fixture.CategoryId)
                .Build();

            var command = new UpdateProductCommand(_fixture.ProductId, updateDto);

            var existingProduct = new ProductBuilder()
                .WithId(_fixture.ProductId)
                .WithName("Old Name")
                .WithCategoryId(_fixture.CategoryId)
                .Build();

            var subCategory = new CategoryBuilder()
                .WithId(_fixture.CategoryId)
                .WithParentId(Guid.NewGuid())
                .Build();

            _productRepo.Setup(x => x.GetByIdAsync(_fixture.ProductId, It.IsAny<CancellationToken>())).ReturnsAsync(existingProduct);
            _categoryRepo.Setup(x => x.GetByIdAsync(_fixture.CategoryId, It.IsAny<CancellationToken>())).ReturnsAsync(subCategory);

            _productRepo.Setup(x => x.AnyAsync(It.IsAny<ISpecification<Product>>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);

            await Assert.ThrowsAsync<ConflictException>(() => _handler.Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_UpdatingImage_ShouldUploadNewImage()
        {
            var imageStream = new MemoryStream(new byte[] { 1, 2, 3 });
            var updateDto = new UpdateProductDtoBuilder()
                .WithCategoryId(_fixture.CategoryId)
                .WithImage(imageStream, "new.jpg", "image/jpeg")
                .Build();

            var command = new UpdateProductCommand(_fixture.ProductId, updateDto);
            var existingProduct = new ProductBuilder().WithId(_fixture.ProductId).WithCategoryId(_fixture.CategoryId).Build();
            var subCategory = new CategoryBuilder().WithId(_fixture.CategoryId).WithParentId(Guid.NewGuid()).Build();

            _productRepo.Setup(x => x.GetByIdAsync(_fixture.ProductId, It.IsAny<CancellationToken>())).ReturnsAsync(existingProduct);
            _categoryRepo.Setup(x => x.GetByIdAsync(_fixture.CategoryId, It.IsAny<CancellationToken>())).ReturnsAsync(subCategory);
            _productRepo.Setup(x => x.AnyAsync(It.IsAny<ISpecification<Product>>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);

            _fileUploadService.Setup(x => x.UploadAsync(It.IsAny<FileUploadRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync("https://cdn.com/new.jpg");

            _mapper.Setup(x => x.Map<ProductResponseDto>(existingProduct))
                .Returns(new ProductResponseDto { ImageUrl = "https://cdn.com/new.jpg" });

            var result = await _handler.Handle(command, CancellationToken.None);

            result.ImageUrl.Should().Be("https://cdn.com/new.jpg");
            _fileUploadService.Verify(x => x.UploadAsync(It.IsAny<FileUploadRequest>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}