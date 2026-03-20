namespace QRDine.Application.Tests.Features.Catalog.ToppingGroups.Commands.DeleteToppingGroup
{
    public class DeleteToppingGroupCommandHandlerTests
    {
        private readonly Mock<IToppingGroupRepository> _toppingGroupRepo;
        private readonly DeleteToppingGroupCommandHandler _handler;
        private readonly CatalogFixture _fixture;

        public DeleteToppingGroupCommandHandlerTests()
        {
            _fixture = new CatalogFixture();
            _toppingGroupRepo = CatalogRepositoryMocks.CreateToppingGroupRepositoryMock();
            _handler = new DeleteToppingGroupCommandHandler(_toppingGroupRepo.Object);
        }

        [Fact]
        public async Task Handle_ToppingGroupNotExists_ShouldThrowNotFoundException()
        {
            var command = new DeleteToppingGroupCommand(_fixture.ToppingGroupId);
            var cancellationToken = CancellationToken.None;

            _toppingGroupRepo
                .Setup(x => x.SingleOrDefaultAsync(It.IsAny<ISingleResultSpecification<ToppingGroup>>(), cancellationToken))
                .ReturnsAsync((ToppingGroup?)null);

            await Assert.ThrowsAsync<NotFoundException>(
                () => _handler.Handle(command, cancellationToken)
            );
        }

        [Fact]
        public async Task Handle_ValidRequest_ShouldSoftDeleteToppingGroupSuccessfully()
        {
            var command = new DeleteToppingGroupCommand(_fixture.ToppingGroupId);
            var cancellationToken = CancellationToken.None;

            var toppingGroup = new ToppingGroup
            {
                Id = _fixture.ToppingGroupId,
                Name = "Cheese Options",
                Toppings = new List<Topping>
                {
                    new Topping { Id = Guid.NewGuid(), Name = "Cheddar" },
                    new Topping { Id = Guid.NewGuid(), Name = "Mozzarella" }
                },
                ProductToppingGroups = new List<ProductToppingGroup>
                {
                    new ProductToppingGroup { Id = Guid.NewGuid(), ProductId = _fixture.ProductId }
                }
            };

            _toppingGroupRepo
                .Setup(x => x.SingleOrDefaultAsync(It.IsAny<ISingleResultSpecification<ToppingGroup>>(), cancellationToken))
                .ReturnsAsync(toppingGroup);

            _toppingGroupRepo
                .Setup(x => x.UpdateAsync(toppingGroup, cancellationToken))
                .Returns(Task.CompletedTask);

            var result = await _handler.Handle(command, cancellationToken);

            toppingGroup.IsDeleted.Should().BeTrue();
            toppingGroup.Toppings.All(t => t.IsDeleted).Should().BeTrue();
            toppingGroup.ProductToppingGroups.Should().BeEmpty();
            result.Should().Be(Unit.Value);
            _toppingGroupRepo.Verify(x => x.UpdateAsync(toppingGroup, cancellationToken), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldSoftDeleteAllToppings()
        {
            var command = new DeleteToppingGroupCommand(_fixture.ToppingGroupId);
            var cancellationToken = CancellationToken.None;

            var toppingGroup = new ToppingGroup
            {
                Id = _fixture.ToppingGroupId,
                Name = "Cheese Options",
                Toppings = new List<Topping>
                {
                    new Topping { Id = Guid.NewGuid(), Name = "Cheddar" },
                    new Topping { Id = Guid.NewGuid(), Name = "Mozzarella" },
                    new Topping { Id = Guid.NewGuid(), Name = "Swiss" }
                },
                ProductToppingGroups = new List<ProductToppingGroup>()
            };

            _toppingGroupRepo
                .Setup(x => x.SingleOrDefaultAsync(It.IsAny<ISingleResultSpecification<ToppingGroup>>(), cancellationToken))
                .ReturnsAsync(toppingGroup);

            _toppingGroupRepo
                .Setup(x => x.UpdateAsync(toppingGroup, cancellationToken))
                .Returns(Task.CompletedTask);

            await _handler.Handle(command, cancellationToken);

            toppingGroup.Toppings.Should().HaveCount(3);
            toppingGroup.Toppings.All(t => t.IsDeleted).Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ShouldClearProductToppingGroupMappings()
        {
            var command = new DeleteToppingGroupCommand(_fixture.ToppingGroupId);
            var cancellationToken = CancellationToken.None;

            var toppingGroup = new ToppingGroup
            {
                Id = _fixture.ToppingGroupId,
                Name = "Cheese Options",
                Toppings = new List<Topping>(),
                ProductToppingGroups = new List<ProductToppingGroup>
                {
                    new ProductToppingGroup { Id = Guid.NewGuid(), ProductId = _fixture.ProductId },
                    new ProductToppingGroup { Id = Guid.NewGuid(), ProductId = Guid.NewGuid() }
                }
            };

            _toppingGroupRepo
                .Setup(x => x.SingleOrDefaultAsync(It.IsAny<ISingleResultSpecification<ToppingGroup>>(), cancellationToken))
                .ReturnsAsync(toppingGroup);

            _toppingGroupRepo
                .Setup(x => x.UpdateAsync(toppingGroup, cancellationToken))
                .Returns(Task.CompletedTask);

            await _handler.Handle(command, cancellationToken);

            toppingGroup.ProductToppingGroups.Should().BeEmpty();
        }
    }
}
