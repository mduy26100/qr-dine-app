namespace QRDine.Application.Tests.Features.Sales.OrderItems.Commands.UpdateOrderItemsStatus
{
    public class UpdateOrderItemsStatusCommandHandlerTests
    {
        private readonly Mock<IOrderItemRepository> _orderItemRepository;
        private readonly UpdateOrderItemsStatusCommandHandler _handler;
        private readonly SalesFixture _fixture;

        public UpdateOrderItemsStatusCommandHandlerTests()
        {
            _fixture = new SalesFixture();
            _orderItemRepository = SalesRepositoryMocks.CreateOrderItemRepositoryMock();

            _handler = new UpdateOrderItemsStatusCommandHandler(
                _orderItemRepository.Object
            );
        }

        [Fact]
        public async Task Handle_NullOrderItemIds_ShouldReturnFalse()
        {
            var command = new UpdateOrderItemsStatusCommand(null!, OrderItemStatus.Preparing);
            var cancellationToken = CancellationToken.None;

            var result = await _handler.Handle(command, cancellationToken);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task Handle_EmptyOrderItemIds_ShouldReturnFalse()
        {
            var command = new UpdateOrderItemsStatusCommand(new List<Guid>(), OrderItemStatus.Preparing);
            var cancellationToken = CancellationToken.None;

            var result = await _handler.Handle(command, cancellationToken);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task Handle_NoOrderItemsFound_ShouldReturnFalse()
        {
            var itemIds = new List<Guid> { Guid.NewGuid() };
            var command = new UpdateOrderItemsStatusCommand(itemIds, OrderItemStatus.Preparing);
            var cancellationToken = CancellationToken.None;

            _orderItemRepository
                .Setup(x => x.ListAsync(It.IsAny<ISpecification<OrderItem>>(), cancellationToken))
                .ReturnsAsync(new List<OrderItem>());

            var result = await _handler.Handle(command, cancellationToken);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task Handle_ValidTransitionPendingToPreparing_ShouldUpdateStatus()
        {
            var itemId = _fixture.OrderItemId;
            var order = new OrderBuilder()
                .WithStatus(OrderStatus.Open)
                .Build();

            var orderItem = new OrderItemBuilder()
                .WithId(itemId)
                .WithStatus(OrderItemStatus.Pending)
                .Build();
            orderItem.Order = order;

            var command = new UpdateOrderItemsStatusCommand(new List<Guid> { itemId }, OrderItemStatus.Preparing);
            var cancellationToken = CancellationToken.None;

            _orderItemRepository
                .Setup(x => x.ListAsync(It.IsAny<ISpecification<OrderItem>>(), cancellationToken))
                .ReturnsAsync(new List<OrderItem> { orderItem });

            _orderItemRepository
                .Setup(x => x.UpdateRangeAsync(It.IsAny<List<OrderItem>>(), cancellationToken))
                .Returns(Task.CompletedTask);

            var result = await _handler.Handle(command, cancellationToken);

            result.Should().BeTrue();
            orderItem.Status.Should().Be(OrderItemStatus.Preparing);
            _orderItemRepository.Verify(
                x => x.UpdateRangeAsync(It.IsAny<List<OrderItem>>(), cancellationToken),
                Times.Once
            );
        }

        [Fact]
        public async Task Handle_ValidTransitionPreparingToServed_ShouldUpdateStatus()
        {
            var itemId = _fixture.OrderItemId;
            var order = new OrderBuilder()
                .WithStatus(OrderStatus.Open)
                .Build();

            var orderItem = new OrderItemBuilder()
                .WithId(itemId)
                .WithStatus(OrderItemStatus.Preparing)
                .Build();
            orderItem.Order = order;

            var command = new UpdateOrderItemsStatusCommand(new List<Guid> { itemId }, OrderItemStatus.Served);
            var cancellationToken = CancellationToken.None;

            _orderItemRepository
                .Setup(x => x.ListAsync(It.IsAny<ISpecification<OrderItem>>(), cancellationToken))
                .ReturnsAsync(new List<OrderItem> { orderItem });

            _orderItemRepository
                .Setup(x => x.UpdateRangeAsync(It.IsAny<List<OrderItem>>(), cancellationToken))
                .Returns(Task.CompletedTask);

            var result = await _handler.Handle(command, cancellationToken);

            result.Should().BeTrue();
            orderItem.Status.Should().Be(OrderItemStatus.Served);
        }

        [Fact]
        public async Task Handle_InvalidTransitionServedToOther_ShouldNotUpdate()
        {
            var itemId = _fixture.OrderItemId;
            var order = new OrderBuilder()
                .WithStatus(OrderStatus.Open)
                .Build();

            var orderItem = new OrderItemBuilder()
                .WithId(itemId)
                .WithStatus(OrderItemStatus.Served)
                .Build();
            orderItem.Order = order;

            var command = new UpdateOrderItemsStatusCommand(new List<Guid> { itemId }, OrderItemStatus.Cancelled);
            var cancellationToken = CancellationToken.None;

            _orderItemRepository
                .Setup(x => x.ListAsync(It.IsAny<ISpecification<OrderItem>>(), cancellationToken))
                .ReturnsAsync(new List<OrderItem> { orderItem });

            var result = await _handler.Handle(command, cancellationToken);

            result.Should().BeTrue();
            orderItem.Status.Should().Be(OrderItemStatus.Served);
            _orderItemRepository.Verify(
                x => x.UpdateRangeAsync(It.IsAny<List<OrderItem>>(), cancellationToken),
                Times.Never
            );
        }

        [Fact]
        public async Task Handle_InvalidTransitionCancelledToOther_ShouldNotUpdate()
        {
            var itemId = _fixture.OrderItemId;
            var order = new OrderBuilder()
                .WithStatus(OrderStatus.Open)
                .Build();

            var orderItem = new OrderItemBuilder()
                .WithId(itemId)
                .WithStatus(OrderItemStatus.Cancelled)
                .Build();
            orderItem.Order = order;

            var command = new UpdateOrderItemsStatusCommand(new List<Guid> { itemId }, OrderItemStatus.Preparing);
            var cancellationToken = CancellationToken.None;

            _orderItemRepository
                .Setup(x => x.ListAsync(It.IsAny<ISpecification<OrderItem>>(), cancellationToken))
                .ReturnsAsync(new List<OrderItem> { orderItem });

            var result = await _handler.Handle(command, cancellationToken);

            result.Should().BeTrue();
            orderItem.Status.Should().Be(OrderItemStatus.Cancelled);
        }

        [Fact]
        public async Task Handle_OrderStatusNotOpen_ShouldNotUpdate()
        {
            var itemId = _fixture.OrderItemId;
            var order = new OrderBuilder()
                .WithStatus(OrderStatus.Paid)
                .Build();

            var orderItem = new OrderItemBuilder()
                .WithId(itemId)
                .WithStatus(OrderItemStatus.Pending)
                .Build();
            orderItem.Order = order;

            var command = new UpdateOrderItemsStatusCommand(new List<Guid> { itemId }, OrderItemStatus.Preparing);
            var cancellationToken = CancellationToken.None;

            _orderItemRepository
                .Setup(x => x.ListAsync(It.IsAny<ISpecification<OrderItem>>(), cancellationToken))
                .ReturnsAsync(new List<OrderItem> { orderItem });

            var result = await _handler.Handle(command, cancellationToken);

            result.Should().BeTrue();
            orderItem.Status.Should().Be(OrderItemStatus.Pending);
            _orderItemRepository.Verify(
                x => x.UpdateRangeAsync(It.IsAny<List<OrderItem>>(), cancellationToken),
                Times.Never
            );
        }

        [Fact]
        public async Task Handle_ItemStatusAlreadyEqualsTargetStatus_ShouldNotUpdate()
        {
            var itemId = _fixture.OrderItemId;
            var order = new OrderBuilder()
                .WithStatus(OrderStatus.Open)
                .Build();

            var orderItem = new OrderItemBuilder()
                .WithId(itemId)
                .WithStatus(OrderItemStatus.Preparing)
                .Build();
            orderItem.Order = order;

            var command = new UpdateOrderItemsStatusCommand(new List<Guid> { itemId }, OrderItemStatus.Preparing);
            var cancellationToken = CancellationToken.None;

            _orderItemRepository
                .Setup(x => x.ListAsync(It.IsAny<ISpecification<OrderItem>>(), cancellationToken))
                .ReturnsAsync(new List<OrderItem> { orderItem });

            var result = await _handler.Handle(command, cancellationToken);

            result.Should().BeTrue();
            _orderItemRepository.Verify(
                x => x.UpdateRangeAsync(It.IsAny<List<OrderItem>>(), cancellationToken),
                Times.Never
            );
        }

        [Fact]
        public async Task Handle_CancellingItemShouldReduceTotalAmount()
        {
            var itemId = _fixture.OrderItemId;
            var itemAmount = 50000m;
            var order = new OrderBuilder()
                .WithStatus(OrderStatus.Open)
                .WithTotalAmount(100000m)
                .Build();

            var orderItem = new OrderItemBuilder()
                .WithId(itemId)
                .WithStatus(OrderItemStatus.Pending)
                .WithAmount(itemAmount)
                .Build();
            orderItem.Order = order;

            var command = new UpdateOrderItemsStatusCommand(new List<Guid> { itemId }, OrderItemStatus.Cancelled);
            var cancellationToken = CancellationToken.None;

            _orderItemRepository
                .Setup(x => x.ListAsync(It.IsAny<ISpecification<OrderItem>>(), cancellationToken))
                .ReturnsAsync(new List<OrderItem> { orderItem });

            _orderItemRepository
                .Setup(x => x.UpdateRangeAsync(It.IsAny<List<OrderItem>>(), cancellationToken))
                .Returns(Task.CompletedTask);

            await _handler.Handle(command, cancellationToken);

            order.TotalAmount.Should().Be(50000m);
        }

        [Fact]
        public async Task Handle_MultipleItemsWithMixedValidTransitions_ShouldUpdateOnlyValidOnes()
        {
            var itemId1 = Guid.NewGuid();
            var itemId2 = Guid.NewGuid();
            var itemId3 = Guid.NewGuid();

            var order = new OrderBuilder()
                .WithStatus(OrderStatus.Open)
                .Build();

            var item1 = new OrderItemBuilder()
                .WithId(itemId1)
                .WithStatus(OrderItemStatus.Pending)
                .Build();
            item1.Order = order;

            var item2 = new OrderItemBuilder()
                .WithId(itemId2)
                .WithStatus(OrderItemStatus.Served)
                .Build();
            item2.Order = order;

            var item3 = new OrderItemBuilder()
                .WithId(itemId3)
                .WithStatus(OrderItemStatus.Preparing)
                .Build();
            item3.Order = order;

            var command = new UpdateOrderItemsStatusCommand(
                new List<Guid> { itemId1, itemId2, itemId3 },
                OrderItemStatus.Served
            );
            var cancellationToken = CancellationToken.None;

            _orderItemRepository
                .Setup(x => x.ListAsync(It.IsAny<ISpecification<OrderItem>>(), cancellationToken))
                .ReturnsAsync(new List<OrderItem> { item1, item2, item3 });

            _orderItemRepository
                .Setup(x => x.UpdateRangeAsync(It.IsAny<IEnumerable<OrderItem>>(), cancellationToken))
                .Callback<IEnumerable<OrderItem>, CancellationToken>((items, ct) =>
                {
                    var itemsList = items.ToList();
                    itemsList.Should().HaveCount(2);
                    itemsList.Should().Contain(item1);
                    itemsList.Should().Contain(item3);
                })
                .Returns(Task.CompletedTask);

            var result = await _handler.Handle(command, cancellationToken);

            result.Should().BeTrue();
            item1.Status.Should().Be(OrderItemStatus.Served);
            item2.Status.Should().Be(OrderItemStatus.Served);
            item3.Status.Should().Be(OrderItemStatus.Served);
        }
    }
}
