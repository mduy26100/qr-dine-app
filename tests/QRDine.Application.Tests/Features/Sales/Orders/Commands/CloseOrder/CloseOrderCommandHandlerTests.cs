namespace QRDine.Application.Tests.Features.Sales.Orders.Commands.CloseOrder
{
    public class CloseOrderCommandHandlerTests
    {
        private readonly Mock<IOrderRepository> _orderRepository;
        private readonly Mock<ITableRepository> _tableRepository;
        private readonly CloseOrderCommandHandler _handler;
        private readonly SalesFixture _fixture;

        public CloseOrderCommandHandlerTests()
        {
            _fixture = new SalesFixture();
            _orderRepository = SalesRepositoryMocks.CreateOrderRepositoryMock();
            _tableRepository = new Mock<ITableRepository>();

            _handler = new CloseOrderCommandHandler(
                _orderRepository.Object,
                _tableRepository.Object
            );
        }

        [Fact]
        public async Task Handle_OrderNotExists_ShouldThrowNotFoundException()
        {
            var command = new CloseOrderCommand(_fixture.OrderId, OrderStatus.Paid);
            var cancellationToken = CancellationToken.None;

            _orderRepository
                .Setup(x => x.SingleOrDefaultAsync(It.IsAny<ISingleResultSpecification<Order>>(), cancellationToken))
                .ReturnsAsync((Order?)null);

            await Assert.ThrowsAsync<NotFoundException>(
                () => _handler.Handle(command, cancellationToken)
            );
        }

        [Fact]
        public async Task Handle_OrderAlreadyPaid_ShouldThrowConflictException()
        {
            var order = new OrderBuilder()
                .WithId(_fixture.OrderId)
                .WithStatus(OrderStatus.Paid)
                .Build();

            var command = new CloseOrderCommand(_fixture.OrderId, OrderStatus.Paid);
            var cancellationToken = CancellationToken.None;

            _orderRepository
                .Setup(x => x.SingleOrDefaultAsync(It.IsAny<ISingleResultSpecification<Order>>(), cancellationToken))
                .ReturnsAsync(order);

            await Assert.ThrowsAsync<ConflictException>(
                () => _handler.Handle(command, cancellationToken)
            );
        }

        [Fact]
        public async Task Handle_OrderAlreadyCancelled_ShouldThrowConflictException()
        {
            var order = new OrderBuilder()
                .WithId(_fixture.OrderId)
                .WithStatus(OrderStatus.Cancelled)
                .Build();

            var command = new CloseOrderCommand(_fixture.OrderId, OrderStatus.Cancelled);
            var cancellationToken = CancellationToken.None;

            _orderRepository
                .Setup(x => x.SingleOrDefaultAsync(It.IsAny<ISingleResultSpecification<Order>>(), cancellationToken))
                .ReturnsAsync(order);

            await Assert.ThrowsAsync<ConflictException>(
                () => _handler.Handle(command, cancellationToken)
            );
        }

        [Fact]
        public async Task Handle_CloseOrderWithPaidStatus_ShouldMarkAllItemsAsServed()
        {
            var orderItems = new List<OrderItem>
            {
                new OrderItemBuilder()
                    .WithOrderId(_fixture.OrderId)
                    .WithStatus(OrderItemStatus.Pending)
                    .Build(),
                new OrderItemBuilder()
                    .WithOrderId(_fixture.OrderId)
                    .WithStatus(OrderItemStatus.Preparing)
                    .Build(),
                new OrderItemBuilder()
                    .WithOrderId(_fixture.OrderId)
                    .WithStatus(OrderItemStatus.Served)
                    .Build(),
                new OrderItemBuilder()
                    .WithOrderId(_fixture.OrderId)
                    .WithStatus(OrderItemStatus.Cancelled)
                    .Build()
            };

            var order = new OrderBuilder()
                .WithId(_fixture.OrderId)
                .WithStatus(OrderStatus.Open)
                .WithOrderItems(orderItems)
                .Build();

            var command = new CloseOrderCommand(_fixture.OrderId, OrderStatus.Paid);
            var cancellationToken = CancellationToken.None;

            _orderRepository
                .Setup(x => x.SingleOrDefaultAsync(It.IsAny<ISingleResultSpecification<Order>>(), cancellationToken))
                .ReturnsAsync(order);

            _tableRepository
                .Setup(x => x.GetByIdAsync(order.TableId, cancellationToken))
                .ReturnsAsync(new Mock<Table>().Object);

            _orderRepository
                .Setup(x => x.UpdateAsync(order, cancellationToken))
                .Returns(Task.CompletedTask);

            _tableRepository
                .Setup(x => x.UpdateAsync(It.IsAny<Table>(), cancellationToken))
                .Returns(Task.CompletedTask);

            var result = await _handler.Handle(command, cancellationToken);

            result.Should().BeTrue();
            orderItems[0].Status.Should().Be(OrderItemStatus.Served);
            orderItems[1].Status.Should().Be(OrderItemStatus.Served);
            orderItems[2].Status.Should().Be(OrderItemStatus.Served);
            orderItems[3].Status.Should().Be(OrderItemStatus.Cancelled);
        }

        [Fact]
        public async Task Handle_CloseOrderWithCancelledStatus_ShouldMarkAllItemsAsCancelled()
        {
            var orderItems = new List<OrderItem>
            {
                new OrderItemBuilder()
                    .WithOrderId(_fixture.OrderId)
                    .WithStatus(OrderItemStatus.Pending)
                    .WithAmount(50000m)
                    .Build(),
                new OrderItemBuilder()
                    .WithOrderId(_fixture.OrderId)
                    .WithStatus(OrderItemStatus.Preparing)
                    .WithAmount(100000m)
                    .Build(),
                new OrderItemBuilder()
                    .WithOrderId(_fixture.OrderId)
                    .WithStatus(OrderItemStatus.Cancelled)
                    .Build()
            };

            var order = new OrderBuilder()
                .WithId(_fixture.OrderId)
                .WithStatus(OrderStatus.Open)
                .WithTotalAmount(150000m)
                .WithOrderItems(orderItems)
                .Build();

            var command = new CloseOrderCommand(_fixture.OrderId, OrderStatus.Cancelled);
            var cancellationToken = CancellationToken.None;

            _orderRepository
                .Setup(x => x.SingleOrDefaultAsync(It.IsAny<ISingleResultSpecification<Order>>(), cancellationToken))
                .ReturnsAsync(order);

            _tableRepository
                .Setup(x => x.GetByIdAsync(order.TableId, cancellationToken))
                .ReturnsAsync(new Mock<Table>().Object);

            _orderRepository
                .Setup(x => x.UpdateAsync(order, cancellationToken))
                .Returns(Task.CompletedTask);

            _tableRepository
                .Setup(x => x.UpdateAsync(It.IsAny<Table>(), cancellationToken))
                .Returns(Task.CompletedTask);

            var result = await _handler.Handle(command, cancellationToken);

            result.Should().BeTrue();
            orderItems[0].Status.Should().Be(OrderItemStatus.Cancelled);
            orderItems[1].Status.Should().Be(OrderItemStatus.Cancelled);
            orderItems[2].Status.Should().Be(OrderItemStatus.Cancelled);
            order.TotalAmount.Should().Be(0m);
        }

        [Fact]
        public async Task Handle_ValidRequest_ShouldUpdateTableStatusToUnoccupied()
        {
            var table = new TableBuilder()
                .WithId(_fixture.TableId)
                .Build();

            var order = new OrderBuilder()
                .WithId(_fixture.OrderId)
                .WithTableId(_fixture.TableId)
                .WithSessionId(_fixture.SessionId)
                .WithStatus(OrderStatus.Open)
                .Build();

            var command = new CloseOrderCommand(_fixture.OrderId, OrderStatus.Paid);
            var cancellationToken = CancellationToken.None;

            _orderRepository
                .Setup(x => x.SingleOrDefaultAsync(It.IsAny<ISingleResultSpecification<Order>>(), cancellationToken))
                .ReturnsAsync(order);

            _tableRepository
                .Setup(x => x.GetByIdAsync(order.TableId, cancellationToken))
                .ReturnsAsync(table);

            _orderRepository
                .Setup(x => x.UpdateAsync(order, cancellationToken))
                .Returns(Task.CompletedTask);

            _tableRepository
                .Setup(x => x.UpdateAsync(It.IsAny<Table>(), cancellationToken))
                .Returns(Task.CompletedTask);

            var result = await _handler.Handle(command, cancellationToken);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ValidRequest_ShouldUpdateOrderStatus()
        {
            var order = new OrderBuilder()
                .WithId(_fixture.OrderId)
                .WithStatus(OrderStatus.Open)
                .Build();

            var command = new CloseOrderCommand(_fixture.OrderId, OrderStatus.Paid);
            var cancellationToken = CancellationToken.None;

            _orderRepository
                .Setup(x => x.SingleOrDefaultAsync(It.IsAny<ISingleResultSpecification<Order>>(), cancellationToken))
                .ReturnsAsync(order);

            _tableRepository
                .Setup(x => x.GetByIdAsync(order.TableId, cancellationToken))
                .ReturnsAsync(new Mock<Table>().Object);

            _orderRepository
                .Setup(x => x.UpdateAsync(order, cancellationToken))
                .Returns(Task.CompletedTask);

            _tableRepository
                .Setup(x => x.UpdateAsync(It.IsAny<Table>(), cancellationToken))
                .Returns(Task.CompletedTask);

            await _handler.Handle(command, cancellationToken);

            order.Status.Should().Be(OrderStatus.Paid);
            _orderRepository.Verify(
                x => x.UpdateAsync(order, cancellationToken),
                Times.Once
            );
        }
    }
}
