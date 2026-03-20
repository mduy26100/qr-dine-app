namespace QRDine.Application.Tests.Features.Catalog.Categories.Commands.DeleteCategory
{
    public class DeleteCategoryCommandHandlerTests
    {
        private readonly Mock<ICategoryRepository> _categoryRepo;
        private readonly DeleteCategoryCommandHandler _handler;
        private readonly CatalogFixture _fixture;

        public DeleteCategoryCommandHandlerTests()
        {
            _fixture = new CatalogFixture();
            _categoryRepo = CatalogRepositoryMocks.CreateCategoryRepositoryMock();
            _handler = new DeleteCategoryCommandHandler(_categoryRepo.Object);
        }

        [Fact]
        public async Task Handle_CategoryNotExists_ShouldThrowNotFoundException()
        {
            var command = new DeleteCategoryCommand(_fixture.CategoryId);
            var cancellationToken = CancellationToken.None;

            _categoryRepo
                .Setup(x => x.GetByIdAsync(_fixture.CategoryId, cancellationToken))
                .ReturnsAsync((Category?)null);

            await Assert.ThrowsAsync<NotFoundException>(
                () => _handler.Handle(command, cancellationToken)
            );
        }

        [Fact]
        public async Task Handle_ValidRequest_ShouldSoftDeleteCategorySuccessfully()
        {
            var command = new DeleteCategoryCommand(_fixture.CategoryId);
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
                .Returns(Task.CompletedTask);

            await _handler.Handle(command, cancellationToken);

            existingCategory.IsDeleted.Should().BeTrue();
            _categoryRepo.Verify(x => x.UpdateAsync(existingCategory, cancellationToken), Times.Once);
        }

        [Fact]
        public async Task Handle_CategoryHasChildren_ShouldThrowBusinessRuleException()
        {
            var command = new DeleteCategoryCommand(_fixture.CategoryId);
            var cancellationToken = CancellationToken.None;

            var existingCategory = new CategoryBuilder()
                .WithId(_fixture.CategoryId)
                .Build();

            _categoryRepo
                .Setup(x => x.GetByIdAsync(_fixture.CategoryId, cancellationToken))
                .ReturnsAsync(existingCategory);

            _categoryRepo
                .Setup(x => x.AnyAsync(It.IsAny<ISpecification<Category>>(), cancellationToken))
                .ReturnsAsync(true);

            await Assert.ThrowsAsync<BusinessRuleException>(
                () => _handler.Handle(command, cancellationToken)
            );
        }

        [Fact]
        public async Task Handle_CategoryHasProducts_ShouldThrowBusinessRuleException()
        {
            var command = new DeleteCategoryCommand(_fixture.CategoryId);
            var cancellationToken = CancellationToken.None;

            var existingCategory = new CategoryBuilder()
                .WithId(_fixture.CategoryId)
                .Build();

            _categoryRepo
                .Setup(x => x.GetByIdAsync(_fixture.CategoryId, cancellationToken))
                .ReturnsAsync(existingCategory);

            var callCount = 0;
            _categoryRepo
                .Setup(x => x.AnyAsync(It.IsAny<ISpecification<Category>>(), cancellationToken))
                .ReturnsAsync(() =>
                {
                    callCount++;
                    return callCount > 1;
                });

            await Assert.ThrowsAsync<BusinessRuleException>(
                () => _handler.Handle(command, cancellationToken)
            );
        }
    }
}
