namespace QRDine.Application.Tests.Features.Catalog.Categories.Commands.UpdateCategory
{
    public class UpdateCategoryCommandHandlerTests
    {
        private readonly Mock<ICategoryRepository> _categoryRepo;
        private readonly Mock<IMapper> _mapper;
        private readonly Mock<IApplicationDbContext> _context;
        private readonly Mock<IDatabaseTransaction> _transaction;
        private readonly UpdateCategoryCommandHandler _handler;
        private readonly CatalogFixture _fixture;

        public UpdateCategoryCommandHandlerTests()
        {
            _fixture = new CatalogFixture();
            _categoryRepo = CatalogRepositoryMocks.CreateCategoryRepositoryMock();
            _mapper = CatalogServiceMocks.CreateMapperMock();
            _context = CatalogServiceMocks.CreateApplicationDbContextMock();
            _transaction = new Mock<IDatabaseTransaction>();

            _context
                .Setup(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(_transaction.Object);

            _handler = new UpdateCategoryCommandHandler(_categoryRepo.Object, _mapper.Object, _context.Object);
        }

        [Fact]
        public async Task Handle_CategoryNotExists_ShouldThrowNotFoundException()
        {
            var updateDto = new UpdateCategoryDtoBuilder().Build();
            var command = new UpdateCategoryCommand(_fixture.CategoryId, updateDto);
            var cancellationToken = CancellationToken.None;

            _categoryRepo
                .Setup(x => x.GetByIdAsync(_fixture.CategoryId, cancellationToken))
                .ReturnsAsync((Category?)null);

            await Assert.ThrowsAsync<NotFoundException>(
                () => _handler.Handle(command, cancellationToken)
            );
        }

        [Fact]
        public async Task Handle_ValidRequest_ShouldUpdateCategorySuccessfully()
        {
            var updateDto = new UpdateCategoryDtoBuilder()
                .WithName("Updated Electronics")
                .Build();

            var command = new UpdateCategoryCommand(_fixture.CategoryId, updateDto);
            var cancellationToken = CancellationToken.None;

            var existingCategory = new CategoryBuilder()
                .WithId(_fixture.CategoryId)
                .WithName("Old Electronics")
                .Build();

            var responseDto = new CategoryResponseDto
            {
                Id = _fixture.CategoryId,
                Name = "Updated Electronics",
                IsActive = true
            };

            _categoryRepo
                .Setup(x => x.GetByIdAsync(_fixture.CategoryId, cancellationToken))
                .ReturnsAsync(existingCategory);

            _categoryRepo
                .Setup(x => x.AnyAsync(It.IsAny<ISingleResultSpecification<Category>>(), cancellationToken))
                .ReturnsAsync(false);

            _categoryRepo
                .Setup(x => x.UpdateAsync(It.IsAny<Category>(), cancellationToken))
                .Returns(Task.CompletedTask);

            _mapper
                .Setup(x => x.Map<CategoryResponseDto>(It.IsAny<Category>()))
                .Returns(responseDto);

            _transaction
                .Setup(x => x.CommitAsync(cancellationToken))
                .Returns(Task.CompletedTask);

            var result = await _handler.Handle(command, cancellationToken);

            result.Should().NotBeNull();
            result.Name.Should().Be("Updated Electronics");
            _categoryRepo.Verify(x => x.UpdateAsync(It.IsAny<Category>(), cancellationToken), Times.Once);
            _transaction.Verify(x => x.CommitAsync(cancellationToken), Times.Once);
        }

        [Fact]
        public async Task Handle_AssignToNewParent_ParentNotExists_ShouldThrowNotFoundException()
        {
            var updateDto = new UpdateCategoryDtoBuilder()
                .WithParentId(_fixture.ParentCategoryId)
                .Build();

            var command = new UpdateCategoryCommand(_fixture.CategoryId, updateDto);
            var cancellationToken = CancellationToken.None;

            var existingCategory = new CategoryBuilder()
                .WithId(_fixture.CategoryId)
                .Build();

            _categoryRepo
                .Setup(x => x.GetByIdAsync(_fixture.CategoryId, cancellationToken))
                .ReturnsAsync(existingCategory);

            _categoryRepo
                .Setup(x => x.GetByIdAsync(_fixture.ParentCategoryId, cancellationToken))
                .ReturnsAsync((Category?)null);

            await Assert.ThrowsAsync<NotFoundException>(
                () => _handler.Handle(command, cancellationToken)
            );
        }

        [Fact]
        public async Task Handle_ChildNameSameAsParent_ShouldThrowBusinessRuleException()
        {
            var parentId = Guid.NewGuid();
            var updateDto = new UpdateCategoryDtoBuilder()
                .WithName("Electronics")
                .WithParentId(parentId)
                .Build();

            var command = new UpdateCategoryCommand(_fixture.CategoryId, updateDto);
            var cancellationToken = CancellationToken.None;

            var existingCategory = new CategoryBuilder()
                .WithId(_fixture.CategoryId)
                .Build();

            var parentCategory = new CategoryBuilder()
                .WithId(parentId)
                .WithName("Electronics")
                .Build();

            _categoryRepo
                .Setup(x => x.GetByIdAsync(_fixture.CategoryId, cancellationToken))
                .ReturnsAsync(existingCategory);

            _categoryRepo
                .Setup(x => x.GetByIdAsync(parentId, cancellationToken))
                .ReturnsAsync(parentCategory);

            await Assert.ThrowsAsync<BusinessRuleException>(
                () => _handler.Handle(command, cancellationToken)
            );
        }

        [Fact]
        public async Task Handle_CategoryBecomesOwnParent_ShouldThrowBusinessRuleException()
        {
            var updateDto = new UpdateCategoryDtoBuilder()
                .WithParentId(_fixture.CategoryId)
                .Build();

            var command = new UpdateCategoryCommand(_fixture.CategoryId, updateDto);
            var cancellationToken = CancellationToken.None;

            var existingCategory = new CategoryBuilder()
                .WithId(_fixture.CategoryId)
                .WithParentId(null)
                .Build();

            _categoryRepo
                .Setup(x => x.GetByIdAsync(_fixture.CategoryId, cancellationToken))
                .ReturnsAsync(existingCategory);

            _categoryRepo
                .Setup(x => x.GetByIdAsync(_fixture.CategoryId, cancellationToken))
                .ReturnsAsync(existingCategory);

            await Assert.ThrowsAsync<BusinessRuleException>(
                () => _handler.Handle(command, cancellationToken)
            );
        }

        [Fact]
        public async Task Handle_CategoryWithChildren_CannotChangeParent_ShouldThrowBusinessRuleException()
        {
            var newParentId = Guid.NewGuid();
            var updateDto = new UpdateCategoryDtoBuilder()
                .WithParentId(newParentId)
                .Build();

            var command = new UpdateCategoryCommand(_fixture.CategoryId, updateDto);
            var cancellationToken = CancellationToken.None;

            var existingCategory = new CategoryBuilder()
                .WithId(_fixture.CategoryId)
                .WithParentId(null)
                .Build();

            var newParent = new CategoryBuilder()
                .WithId(newParentId)
                .WithParentId(null)
                .Build();

            _categoryRepo
                .Setup(x => x.GetByIdAsync(_fixture.CategoryId, cancellationToken))
                .ReturnsAsync(existingCategory);

            _categoryRepo
                .Setup(x => x.GetByIdAsync(newParentId, cancellationToken))
                .ReturnsAsync(newParent);

            _categoryRepo
                .Setup(x => x.AnyAsync(It.IsAny<ISpecification<Category>>(), cancellationToken))
                .ReturnsAsync(true);

            await Assert.ThrowsAsync<BusinessRuleException>(
                () => _handler.Handle(command, cancellationToken)
            );
        }

        [Fact]
        public async Task Handle_DuplicateCategoryName_ShouldThrowConflictException()
        {
            var updateDto = new UpdateCategoryDtoBuilder()
                .WithName("Existing Category")
                .Build();

            var command = new UpdateCategoryCommand(_fixture.CategoryId, updateDto);
            var cancellationToken = CancellationToken.None;

            var existingCategory = new CategoryBuilder()
                .WithId(_fixture.CategoryId)
                .WithName("Old Name")
                .Build();

            _categoryRepo
                .Setup(x => x.GetByIdAsync(_fixture.CategoryId, cancellationToken))
                .ReturnsAsync(existingCategory);

            _categoryRepo
                .Setup(x => x.AnyAsync(It.IsAny<ISpecification<Category>>(), cancellationToken))
                .ReturnsAsync(true);

            await Assert.ThrowsAsync<ConflictException>(
                () => _handler.Handle(command, cancellationToken)
            );
        }

        [Fact]
        public async Task Handle_ChangingDisplayOrder_ShouldShiftOrders()
        {
            var updateDto = new UpdateCategoryDtoBuilder()
                .WithDisplayOrder(3)
                .Build();

            var command = new UpdateCategoryCommand(_fixture.CategoryId, updateDto);
            var cancellationToken = CancellationToken.None;

            var existingCategory = new CategoryBuilder()
                .WithId(_fixture.CategoryId)
                .WithDisplayOrder(1)
                .Build();

            var responseDto = new CategoryResponseDto
            {
                Id = _fixture.CategoryId,
                DisplayOrder = 3
            };

            _categoryRepo
                .Setup(x => x.GetByIdAsync(_fixture.CategoryId, cancellationToken))
                .ReturnsAsync(existingCategory);

            _categoryRepo
                .Setup(x => x.AnyAsync(It.IsAny<ISingleResultSpecification<Category>>(), cancellationToken))
                .ReturnsAsync(false);

            _categoryRepo
                .Setup(x => x.ShiftDisplayOrdersAsync(null, 3, cancellationToken))
                .Returns(Task.CompletedTask);

            _categoryRepo
                .Setup(x => x.UpdateAsync(It.IsAny<Category>(), cancellationToken))
                .Returns(Task.CompletedTask);

            _mapper
                .Setup(x => x.Map<CategoryResponseDto>(It.IsAny<Category>()))
                .Returns(responseDto);

            _transaction
                .Setup(x => x.CommitAsync(cancellationToken))
                .Returns(Task.CompletedTask);

            await _handler.Handle(command, cancellationToken);

            _categoryRepo.Verify(
                x => x.ShiftDisplayOrdersAsync(null, 3, cancellationToken),
                Times.Once
            );
        }

        [Fact]
        public async Task Handle_TransactionRollsBackOnException()
        {
            var updateDto = new UpdateCategoryDtoBuilder().Build();
            var command = new UpdateCategoryCommand(_fixture.CategoryId, updateDto);
            var cancellationToken = CancellationToken.None;

            var existingCategory = new CategoryBuilder()
                .WithId(_fixture.CategoryId)
                .Build();

            _categoryRepo
                .Setup(x => x.GetByIdAsync(_fixture.CategoryId, cancellationToken))
                .ReturnsAsync(existingCategory);

            _categoryRepo
                .Setup(x => x.AnyAsync(It.IsAny<ISingleResultSpecification<Category>>(), cancellationToken))
                .ReturnsAsync(false);

            _categoryRepo
                .Setup(x => x.UpdateAsync(It.IsAny<Category>(), cancellationToken))
                .ThrowsAsync(new Exception("DB error"));

            _transaction
                .Setup(x => x.RollbackAsync(cancellationToken))
                .Returns(Task.CompletedTask);

            await Assert.ThrowsAsync<Exception>(
                () => _handler.Handle(command, cancellationToken)
            );

            _transaction.Verify(x => x.RollbackAsync(cancellationToken), Times.Once);
        }
    }
}
