namespace QRDine.Application.Tests.Features.Catalog.Products.Commands.DeleteProduct
{
    public class DeleteProductCommandHandlerTests
    {
        private readonly Mock<IProductRepository> _productRepo;
        private readonly DeleteProductCommandHandler _handler;
        private readonly CatalogFixture _fixture;

        public DeleteProductCommandHandlerTests()
        {
            _fixture = new CatalogFixture();
            _productRepo = CatalogRepositoryMocks.CreateProductRepositoryMock();
            _handler = new DeleteProductCommandHandler(_productRepo.Object);
        }

        [Fact]
        public async Task Handle_ProductNotExists_ShouldThrowNotFoundException()
        {
            var command = new DeleteProductCommand(_fixture.ProductId);
            var cancellationToken = CancellationToken.None;

            _productRepo
                .Setup(x => x.GetByIdAsync(_fixture.ProductId, cancellationToken))
                .ReturnsAsync((Product?)null);

            await Assert.ThrowsAsync<NotFoundException>(
                () => _handler.Handle(command, cancellationToken)
            );
        }

        [Fact]
        public async Task Handle_AlreadyDeletedProduct_ShouldThrowNotFoundException()
        {
            var command = new DeleteProductCommand(_fixture.ProductId);
            var cancellationToken = CancellationToken.None;

            var deletedProduct = new ProductBuilder()
                .WithId(_fixture.ProductId)
                .WithIsDeleted(true)
                .Build();

            _productRepo
                .Setup(x => x.GetByIdAsync(_fixture.ProductId, cancellationToken))
                .ReturnsAsync(deletedProduct);

            await Assert.ThrowsAsync<NotFoundException>(
                () => _handler.Handle(command, cancellationToken)
            );
        }

        [Fact]
        public async Task Handle_ValidRequest_ShouldSoftDeleteProductSuccessfully()
        {
            var command = new DeleteProductCommand(_fixture.ProductId);
            var cancellationToken = CancellationToken.None;

            var existingProduct = new ProductBuilder()
                .WithId(_fixture.ProductId)
                .Build();

            _productRepo
                .Setup(x => x.GetByIdAsync(_fixture.ProductId, cancellationToken))
                .ReturnsAsync(existingProduct);

            _productRepo
                .Setup(x => x.UpdateAsync(existingProduct, cancellationToken))
                .Returns(Task.CompletedTask);

            await _handler.Handle(command, cancellationToken);

            existingProduct.IsDeleted.Should().BeTrue();
            _productRepo.Verify(x => x.UpdateAsync(existingProduct, cancellationToken), Times.Once);
        }
    }
}
