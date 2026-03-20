namespace QRDine.Application.Tests.Features.Catalog.Categories.Commands.CreateCategory
{
    public class CreateCategoryCommandHandlerTests
    {
        private readonly Mock<ICategoryRepository> _categoryRepo;
        private readonly Mock<IMapper> _mapper;
        private readonly Mock<IApplicationDbContext> _context;
        private readonly Mock<IDatabaseTransaction> _transaction;
        private readonly CreateCategoryCommandHandler _handler;
        private readonly CatalogFixture _fixture;

        public CreateCategoryCommandHandlerTests()
        {
            _fixture = new CatalogFixture();
            _categoryRepo = CatalogRepositoryMocks.CreateCategoryRepositoryMock();
            _mapper = CatalogServiceMocks.CreateMapperMock();
            _context = CatalogServiceMocks.CreateApplicationDbContextMock();
            _transaction = new Mock<IDatabaseTransaction>();

            _context
                .Setup(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(_transaction.Object);

            _handler = new CreateCategoryCommandHandler(_categoryRepo.Object, _mapper.Object, _context.Object);
        }

        [Fact]
        public async Task Handle_ValidRequest_ShouldCreateCategorySuccessfully()
        {
            var createDto = new CreateCategoryDtoBuilder()
                .WithName("Electronics")
                .WithDescription("Electronic products")
                .WithDisplayOrder(1)
                .Build();

            var command = new CreateCategoryCommand(createDto);
            var cancellationToken = CancellationToken.None;

            var createdCategory = new CategoryBuilder()
                .WithId(_fixture.CategoryId)
                .WithMerchantId(_fixture.MerchantId)
                .WithName("Electronics")
                .WithDisplayOrder(1)
                .Build();

            var responseDto = new CategoryResponseDto
            {
                Id = _fixture.CategoryId,
                Name = "Electronics",
                Description = "Electronic products",
                DisplayOrder = 1,
                IsActive = true,
                ParentId = null
            };

            _categoryRepo
                .Setup(x => x.AnyAsync(It.IsAny<ISingleResultSpecification<Category>>(), cancellationToken))
                .ReturnsAsync(false);

            _categoryRepo
                .Setup(x => x.GetMaxDisplayOrderAsync(null, cancellationToken))
                .ReturnsAsync(0);

            _categoryRepo
                .Setup(x => x.AddAsync(It.IsAny<Category>(), cancellationToken))
                .ReturnsAsync((Category c, CancellationToken ct) => c);

            _mapper
                .Setup(x => x.Map<Category>(createDto))
                .Returns(createdCategory);

            _mapper
                .Setup(x => x.Map<CategoryResponseDto>(createdCategory))
                .Returns(responseDto);

            _transaction
                .Setup(x => x.CommitAsync(cancellationToken))
                .Returns(Task.CompletedTask);

            var result = await _handler.Handle(command, cancellationToken);

            result.Should().NotBeNull();
            result.Id.Should().Be(_fixture.CategoryId);
            result.Name.Should().Be("Electronics");
            _categoryRepo.Verify(x => x.AddAsync(It.IsAny<Category>(), cancellationToken), Times.Once);
            _transaction.Verify(x => x.CommitAsync(cancellationToken), Times.Once);
        }

        [Fact]
        public async Task Handle_WithParentId_ParentNotExists_ShouldThrowNotFoundException()
        {
            var createDto = new CreateCategoryDtoBuilder()
                .WithName("Laptop")
                .WithParentId(_fixture.ParentCategoryId)
                .Build();

            var command = new CreateCategoryCommand(createDto);
            var cancellationToken = CancellationToken.None;

            _categoryRepo
                .Setup(x => x.GetByIdAsync(_fixture.ParentCategoryId, cancellationToken))
                .ReturnsAsync((Category?)null);

            await Assert.ThrowsAsync<NotFoundException>(
                () => _handler.Handle(command, cancellationToken)
            );
        }

        [Fact]
        public async Task Handle_WithParentIdThatHasParent_ShouldThrowBusinessRuleException()
        {
            var parentId = Guid.NewGuid();
            var grandparentId = Guid.NewGuid();

            var createDto = new CreateCategoryDtoBuilder()
                .WithName("Laptop")
                .WithParentId(parentId)
                .Build();

            var command = new CreateCategoryCommand(createDto);
            var cancellationToken = CancellationToken.None;

            var parentCategory = new CategoryBuilder()
                .WithId(parentId)
                .WithParentId(grandparentId)
                .Build();

            _categoryRepo
                .Setup(x => x.GetByIdAsync(parentId, cancellationToken))
                .ReturnsAsync(parentCategory);

            await Assert.ThrowsAsync<BusinessRuleException>(
                () => _handler.Handle(command, cancellationToken)
            );
        }

        [Fact]
        public async Task Handle_DuplicateCategoryNameAtSameLevel_ShouldThrowConflictException()
        {
            var createDto = new CreateCategoryDtoBuilder()
                .WithName("Electronics")
                .Build();

            var command = new CreateCategoryCommand(createDto);
            var cancellationToken = CancellationToken.None;

            _categoryRepo
                .Setup(x => x.AnyAsync(It.IsAny<ISpecification<Category>>(), cancellationToken))
                .ReturnsAsync(true);

            await Assert.ThrowsAsync<ConflictException>(
                () => _handler.Handle(command, cancellationToken)
            );
        }

        [Fact]
        public async Task Handle_WithDisplayOrderZero_ShouldCalculateFromMaxOrder()
        {
            var createDto = new CreateCategoryDtoBuilder()
                .WithName("Electronics")
                .WithDisplayOrder(0)
                .Build();

            var command = new CreateCategoryCommand(createDto);
            var cancellationToken = CancellationToken.None;

            var createdCategory = new CategoryBuilder()
                .WithName("Electronics")
                .WithDisplayOrder(0)
                .Build();

            var responseDto = new CategoryResponseDto
            {
                Id = createdCategory.Id,
                Name = "Electronics",
                DisplayOrder = 4,
                IsActive = true
            };

            _categoryRepo
                .Setup(x => x.AnyAsync(It.IsAny<ISingleResultSpecification<Category>>(), cancellationToken))
                .ReturnsAsync(false);

            _categoryRepo
                .Setup(x => x.GetMaxDisplayOrderAsync(null, cancellationToken))
                .ReturnsAsync(3);

            _mapper
                .Setup(x => x.Map<Category>(createDto))
                .Returns(createdCategory);

            _mapper
                .Setup(x => x.Map<CategoryResponseDto>(It.IsAny<Category>()))
                .Returns(responseDto);

            _categoryRepo
                .Setup(x => x.AddAsync(It.IsAny<Category>(), cancellationToken))
                .ReturnsAsync((Category c, CancellationToken ct) => c);

            _transaction
                .Setup(x => x.CommitAsync(cancellationToken))
                .Returns(Task.CompletedTask);

            var result = await _handler.Handle(command, cancellationToken);

            result.DisplayOrder.Should().Be(4);
        }

        [Fact]
        public async Task Handle_WithExplicitDisplayOrder_ShouldShiftOrders()
        {
            var createDto = new CreateCategoryDtoBuilder()
                .WithName("Electronics")
                .WithDisplayOrder(2)
                .Build();

            var command = new CreateCategoryCommand(createDto);
            var cancellationToken = CancellationToken.None;

            var createdCategory = new CategoryBuilder()
                .WithName("Electronics")
                .WithDisplayOrder(2)
                .Build();

            var responseDto = new CategoryResponseDto
            {
                Id = createdCategory.Id,
                Name = "Electronics",
                DisplayOrder = 2
            };

            _categoryRepo
                .Setup(x => x.AnyAsync(It.IsAny<ISingleResultSpecification<Category>>(), cancellationToken))
                .ReturnsAsync(false);

            _categoryRepo
                .Setup(x => x.ShiftDisplayOrdersAsync(null, 2, cancellationToken))
                .Returns(Task.CompletedTask);

            _mapper
                .Setup(x => x.Map<Category>(createDto))
                .Returns(createdCategory);

            _mapper
                .Setup(x => x.Map<CategoryResponseDto>(It.IsAny<Category>()))
                .Returns(responseDto);

            _categoryRepo
                .Setup(x => x.AddAsync(It.IsAny<Category>(), cancellationToken))
                .ReturnsAsync((Category c, CancellationToken ct) => c);

            _transaction
                .Setup(x => x.CommitAsync(cancellationToken))
                .Returns(Task.CompletedTask);

            await _handler.Handle(command, cancellationToken);

            _categoryRepo.Verify(
                x => x.ShiftDisplayOrdersAsync(null, 2, cancellationToken),
                Times.Once
            );
        }

        [Fact]
        public async Task Handle_TransactionRollsBackOnException()
        {
            var createDto = new CreateCategoryDtoBuilder()
                .WithName("Electronics")
                .Build();

            var command = new CreateCategoryCommand(createDto);
            var cancellationToken = CancellationToken.None;

            var createdCategory = new CategoryBuilder()
                .WithName("Electronics")
                .Build();

            _categoryRepo
                .Setup(x => x.AnyAsync(It.IsAny<ISingleResultSpecification<Category>>(), cancellationToken))
                .ReturnsAsync(false);

            _categoryRepo
                .Setup(x => x.GetMaxDisplayOrderAsync(null, cancellationToken))
                .ReturnsAsync(0);

            _mapper
                .Setup(x => x.Map<Category>(createDto))
                .Returns(createdCategory);

            _categoryRepo
                .Setup(x => x.AddAsync(It.IsAny<Category>(), cancellationToken))
                .ThrowsAsync(new Exception("DB error"));

            _transaction
                .Setup(x => x.RollbackAsync(cancellationToken))
                .Returns(Task.CompletedTask);

            await Assert.ThrowsAsync<Exception>(() => _handler.Handle(command, cancellationToken));

            _transaction.Verify(x => x.RollbackAsync(cancellationToken), Times.Once);
        }
    }
}
