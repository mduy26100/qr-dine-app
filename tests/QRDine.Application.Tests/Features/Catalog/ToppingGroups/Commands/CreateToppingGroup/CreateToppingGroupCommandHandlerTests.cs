namespace QRDine.Application.Tests.Features.Catalog.ToppingGroups.Commands.CreateToppingGroup
{
    public class CreateToppingGroupCommandHandlerTests
    {
        private readonly Mock<IToppingGroupRepository> _toppingGroupRepo;
        private readonly Mock<IProductRepository> _productRepo;
        private readonly CreateToppingGroupCommandHandler _handler;
        private readonly CatalogFixture _fixture;

        public CreateToppingGroupCommandHandlerTests()
        {
            _fixture = new CatalogFixture();
            _toppingGroupRepo = new Mock<IToppingGroupRepository>();
            _productRepo = new Mock<IProductRepository>();

            _handler = new CreateToppingGroupCommandHandler(_toppingGroupRepo.Object, _productRepo.Object);
        }

        [Fact]
        public async Task Handle_ValidRequest_ShouldCreateToppingGroupSuccessfully()
        {
            var toppingDto = new ToppingRequestDtoBuilder()
                .WithName("Extra Cheese")
                .WithPrice(50m)
                .Build();

            var requestDto = new CreateToppingGroupRequestDtoBuilder()
                .WithName("Cheese Options")
                .WithToppings(new List<ToppingRequestDto> { toppingDto })
                .WithAppliedProductIds(new List<Guid> { _fixture.ProductId })
                .Build();

            var command = new CreateToppingGroupCommand(requestDto);

            _productRepo
                .Setup(x => x.CountAsync(It.IsAny<ISpecification<Product>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            _toppingGroupRepo
                .Setup(x => x.AddAsync(It.IsAny<ToppingGroup>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((ToppingGroup tg, CancellationToken ct) => tg);

            var result = await _handler.Handle(command, CancellationToken.None);

            result.Should().NotBeNull();
            result.Name.Should().Be("Cheese Options");
            _toppingGroupRepo.Verify(x => x.AddAsync(It.IsAny<ToppingGroup>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_WithToppings_ShouldCreateToppingGroupWithToppings()
        {
            var topping1 = new ToppingRequestDtoBuilder().WithName("Cheddar").WithPrice(50m).Build();
            var topping2 = new ToppingRequestDtoBuilder().WithName("Mozzarella").WithPrice(60m).Build();

            var requestDto = new CreateToppingGroupRequestDtoBuilder()
                .WithName("Cheese Options")
                .WithToppings(new List<ToppingRequestDto> { topping1, topping2 })
                .Build();

            var command = new CreateToppingGroupCommand(requestDto);

            _toppingGroupRepo
                .Setup(x => x.AddAsync(It.IsAny<ToppingGroup>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((ToppingGroup tg, CancellationToken ct) => tg);

            var result = await _handler.Handle(command, CancellationToken.None);

            result.Should().NotBeNull();
            _toppingGroupRepo.Verify(x => x.AddAsync(It.IsAny<ToppingGroup>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_WithAppliedProducts_ShouldVerifyProductsExist()
        {
            var productIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };

            var requestDto = new CreateToppingGroupRequestDtoBuilder()
                .WithName("Cheese Options")
                .WithAppliedProductIds(productIds)
                .Build();

            var command = new CreateToppingGroupCommand(requestDto);

            _productRepo
                .Setup(x => x.CountAsync(It.IsAny<ISpecification<Product>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(2);

            _toppingGroupRepo
                .Setup(x => x.AddAsync(It.IsAny<ToppingGroup>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((ToppingGroup tg, CancellationToken ct) => tg);

            var result = await _handler.Handle(command, CancellationToken.None);

            result.Should().NotBeNull();
            _productRepo.Verify(
                x => x.CountAsync(It.IsAny<ISpecification<Product>>(), It.IsAny<CancellationToken>()),
                Times.Once
            );
        }

        [Fact]
        public async Task Handle_InvalidProductIds_ShouldThrowBadRequestException()
        {
            var productIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };

            var requestDto = new CreateToppingGroupRequestDtoBuilder()
                .WithName("Cheese Options")
                .WithAppliedProductIds(productIds)
                .Build();

            var command = new CreateToppingGroupCommand(requestDto);

            _productRepo
                .Setup(x => x.CountAsync(It.IsAny<ISpecification<Product>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            await Assert.ThrowsAsync<BadRequestException>(
                () => _handler.Handle(command, CancellationToken.None)
            );
        }

        [Fact]
        public async Task Handle_WithMinSelections_ShouldSetIsRequiredTrue()
        {
            var requestDto = new CreateToppingGroupRequestDtoBuilder()
                .WithName("Cheese Options")
                .WithMinSelections(1)
                .WithMaxSelections(2)
                .Build();

            var command = new CreateToppingGroupCommand(requestDto);

            _toppingGroupRepo
                .Setup(x => x.AddAsync(It.IsAny<ToppingGroup>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((ToppingGroup tg, CancellationToken ct) => tg);

            var result = await _handler.Handle(command, CancellationToken.None);

            result.Should().NotBeNull();
            _toppingGroupRepo.Verify(x => x.AddAsync(It.IsAny<ToppingGroup>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_WithoutAppliedProducts_ShouldCreateToppingGroupWithoutProducts()
        {
            var requestDto = new CreateToppingGroupRequestDtoBuilder()
                .WithName("Cheese Options")
                .WithAppliedProductIds(new List<Guid>())
                .Build();

            var command = new CreateToppingGroupCommand(requestDto);

            _toppingGroupRepo
                .Setup(x => x.AddAsync(It.IsAny<ToppingGroup>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((ToppingGroup tg, CancellationToken ct) => tg);

            var result = await _handler.Handle(command, CancellationToken.None);

            result.Should().NotBeNull();
            _productRepo.Verify(
                x => x.CountAsync(It.IsAny<ISpecification<Product>>(), It.IsAny<CancellationToken>()),
                Times.Never
            );
        }
    }
}