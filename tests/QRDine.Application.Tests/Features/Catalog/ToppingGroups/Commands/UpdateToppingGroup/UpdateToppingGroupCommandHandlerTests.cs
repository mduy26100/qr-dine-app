namespace QRDine.Application.Tests.Features.Catalog.ToppingGroups.Commands.UpdateToppingGroup
{
    public class UpdateToppingGroupCommandHandlerTests
    {
        private readonly Mock<IToppingGroupRepository> _toppingGroupRepo;
        private readonly Mock<IProductRepository> _productRepo;
        private readonly UpdateToppingGroupCommandHandler _handler;
        private readonly CatalogFixture _fixture;

        public UpdateToppingGroupCommandHandlerTests()
        {
            _fixture = new CatalogFixture();
            _toppingGroupRepo = new Mock<IToppingGroupRepository>();
            _productRepo = new Mock<IProductRepository>();

            _handler = new UpdateToppingGroupCommandHandler(_toppingGroupRepo.Object, _productRepo.Object);
        }

        [Fact]
        public async Task Handle_ToppingGroupNotExists_ShouldThrowNotFoundException()
        {
            var requestDto = new UpdateToppingGroupRequestDtoBuilder().Build();
            var command = new UpdateToppingGroupCommand(_fixture.ToppingGroupId, requestDto);

            _toppingGroupRepo
                .Setup(x => x.SingleOrDefaultAsync(It.IsAny<ISingleResultSpecification<ToppingGroup>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((ToppingGroup?)null);

            await Assert.ThrowsAsync<NotFoundException>(
                () => _handler.Handle(command, CancellationToken.None)
            );
        }

        [Fact]
        public async Task Handle_ValidRequest_ShouldUpdateToppingGroupSuccessfully()
        {
            var requestDto = new UpdateToppingGroupRequestDtoBuilder()
                .WithName("Updated Cheese Options")
                .Build();

            var command = new UpdateToppingGroupCommand(_fixture.ToppingGroupId, requestDto);

            var toppingGroup = new ToppingGroup
            {
                Id = _fixture.ToppingGroupId,
                Name = "Old Cheese Options",
                Toppings = new List<Topping>(),
                ProductToppingGroups = new List<ProductToppingGroup>()
            };

            _toppingGroupRepo
                .Setup(x => x.SingleOrDefaultAsync(It.IsAny<ISingleResultSpecification<ToppingGroup>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(toppingGroup);

            _toppingGroupRepo
                .Setup(x => x.UpdateAsync(toppingGroup, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await _handler.Handle(command, CancellationToken.None);

            toppingGroup.Name.Should().Be("Updated Cheese Options");
            result.Should().Be(Unit.Value);
            _toppingGroupRepo.Verify(x => x.UpdateAsync(toppingGroup, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_InvalidProductIds_ShouldThrowBadRequestException()
        {
            var requestDto = new UpdateToppingGroupRequestDtoBuilder()
                .AddAppliedProductId(_fixture.ProductId)
                .Build();

            var command = new UpdateToppingGroupCommand(_fixture.ToppingGroupId, requestDto);

            var toppingGroup = new ToppingGroup
            {
                Id = _fixture.ToppingGroupId,
                Toppings = new List<Topping>(),
                ProductToppingGroups = new List<ProductToppingGroup>()
            };

            _toppingGroupRepo
                .Setup(x => x.SingleOrDefaultAsync(It.IsAny<ISingleResultSpecification<ToppingGroup>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(toppingGroup);

            _productRepo
                .Setup(x => x.CountAsync(It.IsAny<ISpecification<Product>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(0);

            await Assert.ThrowsAsync<BadRequestException>(
                () => _handler.Handle(command, CancellationToken.None)
            );
        }

        [Fact]
        public async Task Handle_RemovingToppingsShouldSoftDelete()
        {
            var toppingId1 = Guid.NewGuid();
            var toppingId2 = Guid.NewGuid();

            var requestDto = new UpdateToppingGroupRequestDtoBuilder()
                .WithToppings(new List<UpdateToppingRequestDto>())
                .Build();

            var command = new UpdateToppingGroupCommand(_fixture.ToppingGroupId, requestDto);

            var toppingGroup = new ToppingGroup
            {
                Id = _fixture.ToppingGroupId,
                Name = "Cheese Options",
                Toppings = new List<Topping>
                {
                    new Topping { Id = toppingId1, Name = "Cheddar", IsDeleted = false },
                    new Topping { Id = toppingId2, Name = "Mozzarella", IsDeleted = false }
                },
                ProductToppingGroups = new List<ProductToppingGroup>()
            };

            _toppingGroupRepo
                .Setup(x => x.SingleOrDefaultAsync(It.IsAny<ISingleResultSpecification<ToppingGroup>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(toppingGroup);

            _toppingGroupRepo
                .Setup(x => x.UpdateAsync(toppingGroup, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await _handler.Handle(command, CancellationToken.None);

            toppingGroup.Toppings.All(t => t.IsDeleted).Should().BeTrue();
        }

        [Fact]
        public async Task Handle_UpdatingExistingTopping_ShouldUpdateFields()
        {
            var toppingId = Guid.NewGuid();
            var updatedTopping = new UpdateToppingRequestDto
            {
                Id = toppingId,
                Name = "Premium Cheddar",
                Price = 75m,
                DisplayOrder = 1,
                IsAvailable = true
            };

            var requestDto = new UpdateToppingGroupRequestDtoBuilder()
                .WithToppings(new List<UpdateToppingRequestDto> { updatedTopping })
                .Build();

            var command = new UpdateToppingGroupCommand(_fixture.ToppingGroupId, requestDto);

            var toppingGroup = new ToppingGroup
            {
                Id = _fixture.ToppingGroupId,
                Name = "Cheese Options",
                Toppings = new List<Topping>
                {
                    new Topping { Id = toppingId, Name = "Regular Cheddar", Price = 50m, IsDeleted = false }
                },
                ProductToppingGroups = new List<ProductToppingGroup>()
            };

            _toppingGroupRepo
                .Setup(x => x.SingleOrDefaultAsync(It.IsAny<ISingleResultSpecification<ToppingGroup>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(toppingGroup);

            _toppingGroupRepo
                .Setup(x => x.UpdateAsync(toppingGroup, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await _handler.Handle(command, CancellationToken.None);

            var toppingsList = toppingGroup.Toppings.ToList();
            toppingsList[0].Name.Should().Be("Premium Cheddar");
            toppingsList[0].Price.Should().Be(75m);
        }

        [Fact]
        public async Task Handle_ResurrectingDeletedTopping_ShouldRestoreTopping()
        {
            var toppingName = "Cheddar";
            var requestDto = new UpdateToppingGroupRequestDtoBuilder()
                .WithToppings(new List<UpdateToppingRequestDto>
                {
                    new UpdateToppingRequestDto { Id = null, Name = toppingName, Price = 50m }
                })
                .Build();

            var command = new UpdateToppingGroupCommand(_fixture.ToppingGroupId, requestDto);

            var toppingGroup = new ToppingGroup
            {
                Id = _fixture.ToppingGroupId,
                Name = "Cheese Options",
                Toppings = new List<Topping>
                {
                    new Topping { Id = Guid.NewGuid(), Name = toppingName, IsDeleted = true }
                },
                ProductToppingGroups = new List<ProductToppingGroup>()
            };

            _toppingGroupRepo
                .Setup(x => x.SingleOrDefaultAsync(It.IsAny<ISingleResultSpecification<ToppingGroup>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(toppingGroup);

            await _handler.Handle(command, CancellationToken.None);

            var toppingsList = toppingGroup.Toppings.ToList();
            toppingsList[0].IsDeleted.Should().BeFalse();
        }

        [Fact]
        public async Task Handle_RemovingProductMapping_ShouldUnassignProduct()
        {
            var product1Id = Guid.NewGuid();
            var product2Id = Guid.NewGuid();

            var requestDto = new UpdateToppingGroupRequestDtoBuilder()
                .WithAppliedProductIds(new List<Guid> { product1Id })
                .Build();

            var command = new UpdateToppingGroupCommand(_fixture.ToppingGroupId, requestDto);

            var toppingGroup = new ToppingGroup
            {
                Id = _fixture.ToppingGroupId,
                Toppings = new List<Topping>(),
                ProductToppingGroups = new List<ProductToppingGroup>
                {
                    new ProductToppingGroup { Id = Guid.NewGuid(), ProductId = product1Id },
                    new ProductToppingGroup { Id = Guid.NewGuid(), ProductId = product2Id }
                }
            };

            _toppingGroupRepo
                .Setup(x => x.SingleOrDefaultAsync(It.IsAny<ISingleResultSpecification<ToppingGroup>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(toppingGroup);

            _productRepo
                .Setup(x => x.CountAsync(It.IsAny<ISpecification<Product>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            await _handler.Handle(command, CancellationToken.None);

            toppingGroup.ProductToppingGroups.Should().HaveCount(1);
            toppingGroup.ProductToppingGroups.First().ProductId.Should().Be(product1Id);
        }

        [Fact]
        public async Task Handle_AssigningNewProduct_ShouldAddProductMapping()
        {
            var product1Id = Guid.NewGuid();
            var product2Id = Guid.NewGuid();

            var requestDto = new UpdateToppingGroupRequestDtoBuilder()
                .WithAppliedProductIds(new List<Guid> { product1Id, product2Id })
                .Build();

            var command = new UpdateToppingGroupCommand(_fixture.ToppingGroupId, requestDto);

            var toppingGroup = new ToppingGroup
            {
                Id = _fixture.ToppingGroupId,
                Toppings = new List<Topping>(),
                ProductToppingGroups = new List<ProductToppingGroup>
                {
                    new ProductToppingGroup { Id = Guid.NewGuid(), ProductId = product1Id }
                }
            };

            _toppingGroupRepo
                .Setup(x => x.SingleOrDefaultAsync(It.IsAny<ISingleResultSpecification<ToppingGroup>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(toppingGroup);

            _productRepo
                .Setup(x => x.CountAsync(It.IsAny<ISpecification<Product>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(2);

            await _handler.Handle(command, CancellationToken.None);

            toppingGroup.ProductToppingGroups.Should().HaveCount(2);
        }
    }
}