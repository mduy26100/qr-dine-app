namespace QRDine.Application.Tests.Features.Sales.Orders.Commands.StorefrontCreateOrder
{
    public class StorefrontCreateOrderCommandHandlerTests
    {
        private readonly Mock<IOrderCreationService> _orderCreationService;
        private readonly Mock<IOrderNotificationService> _notificationService;
        private readonly Mock<IMapper> _mapper;
        private readonly StorefrontCreateOrderCommandHandler _handler;
        private readonly SalesFixture _fixture;

        public StorefrontCreateOrderCommandHandlerTests()
        {
            _fixture = new SalesFixture();
            _orderCreationService = SalesServiceMocks.CreateOrderCreationServiceMock();
            _notificationService = SalesServiceMocks.CreateOrderNotificationServiceMock();
            _mapper = SalesServiceMocks.CreateMapperMock();

            _handler = new StorefrontCreateOrderCommandHandler(
                _orderCreationService.Object,
                _notificationService.Object,
                _mapper.Object
            );
        }

        [Fact]
        public async Task Handle_ValidRequest_ShouldCreateOrderSuccessfully()
        {
            var createDto = new StorefrontCreateOrderDtoBuilder()
                .WithTableId(_fixture.TableId)
                .WithCustomerName("Nguyen Van A")
                .WithCustomerPhone("0912345678")
                .Build();

            var command = new StorefrontCreateOrderCommand(_fixture.MerchantId, createDto);
            var cancellationToken = CancellationToken.None;

            var createdOrder = new OrderBuilder()
                .WithId(_fixture.OrderId)
                .WithMerchantId(_fixture.MerchantId)
                .WithTableId(_fixture.TableId)
                .WithSessionId(_fixture.SessionId)
                .WithCustomerName("Nguyen Van A")
                .WithCustomerPhone("0912345678")
                .Build();

            var responseDto = new OrderResponseDto
            {
                Id = _fixture.OrderId,
                OrderCode = "ORD001",
                TableName = "Bàn 1",
                Status = OrderStatus.Open.ToString(),
                TotalAmount = 100000m,
                CreatedOn = DateTime.Now,
                SessionId = _fixture.SessionId
            };

            _orderCreationService
                .Setup(x => x.CreateOrAppendOrderAsync(It.IsAny<OrderCreationDto>(), cancellationToken))
                .ReturnsAsync(createdOrder);

            _notificationService
                .Setup(x => x.NotifyOrderUpdatedAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _mapper
                .Setup(x => x.Map<OrderResponseDto>(createdOrder))
                .Returns(responseDto);

            var result = await _handler.Handle(command, cancellationToken);

            result.Should().NotBeNull();
            result.Id.Should().Be(_fixture.OrderId);
            result.SessionId.Should().Be(_fixture.SessionId);
            _orderCreationService.Verify(
                x => x.CreateOrAppendOrderAsync(It.IsAny<OrderCreationDto>(), cancellationToken),
                Times.Once
            );
        }

        [Fact]
        public async Task Handle_ValidRequest_ShouldNotifyOrderUpdated()
        {
            var createDto = new StorefrontCreateOrderDtoBuilder()
                .WithTableId(_fixture.TableId)
                .Build();

            var command = new StorefrontCreateOrderCommand(_fixture.MerchantId, createDto);
            var cancellationToken = CancellationToken.None;

            var createdOrder = new OrderBuilder()
                .WithId(_fixture.OrderId)
                .WithMerchantId(_fixture.MerchantId)
                .WithTableId(_fixture.TableId)
                .WithTableName("Bàn 5")
                .Build();

            _orderCreationService
                .Setup(x => x.CreateOrAppendOrderAsync(It.IsAny<OrderCreationDto>(), cancellationToken))
                .ReturnsAsync(createdOrder);

            _notificationService
                .Setup(x => x.NotifyOrderUpdatedAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _mapper
                .Setup(x => x.Map<OrderResponseDto>(createdOrder))
                .Returns(new OrderResponseDto());

            await _handler.Handle(command, CancellationToken.None);

            _notificationService.Verify(
                x => x.NotifyOrderUpdatedAsync(
                    _fixture.MerchantId,
                    _fixture.TableId,
                    "Bàn 5",
                    It.IsAny<CancellationToken>()
                ),
                Times.Once
            );
        }

        [Fact]
        public async Task Handle_ValidRequest_ShouldPassCorrectOrderCreationDto()
        {
            var customerName = "Tran Thi B";
            var customerPhone = "0987654321";
            var note = "Tặng thêm nước chanh";
            var productId = Guid.NewGuid();

            var createDto = new StorefrontCreateOrderDtoBuilder()
                .WithTableId(_fixture.TableId)
                .WithCustomerName(customerName)
                .WithCustomerPhone(customerPhone)
                .WithNote(note)
                .WithItems(new List<StorefrontCreateOrderItemDto>
                {
                    new StorefrontCreateOrderItemDto
                    {
                        ProductId = productId,
                        Quantity = 1,
                        Note = null,
                        SelectedToppingIds = new List<Guid>()
                    }
                })
                .Build();

            var command = new StorefrontCreateOrderCommand(_fixture.MerchantId, createDto);
            var cancellationToken = CancellationToken.None;

            var createdOrder = new OrderBuilder().Build();

            _orderCreationService
                .Setup(x => x.CreateOrAppendOrderAsync(It.IsAny<OrderCreationDto>(), cancellationToken))
                .ReturnsAsync(createdOrder);

            _notificationService
                .Setup(x => x.NotifyOrderUpdatedAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _mapper
                .Setup(x => x.Map<OrderResponseDto>(createdOrder))
                .Returns(new OrderResponseDto());

            await _handler.Handle(command, cancellationToken);

            _orderCreationService.Verify(
                x => x.CreateOrAppendOrderAsync(
                    It.Is<OrderCreationDto>(dto =>
                        dto.MerchantId == _fixture.MerchantId &&
                        dto.TableId == _fixture.TableId &&
                        dto.CustomerName == customerName &&
                        dto.CustomerPhone == customerPhone &&
                        dto.Note == note &&
                        dto.Items.Count == 1 &&
                        dto.Items[0].ProductId == productId
                    ),
                    cancellationToken
                ),
                Times.Once
            );
        }

        [Fact]
        public async Task Handle_ServiceThrowsNotFoundException_ShouldPropagate()
        {
            var createDto = new StorefrontCreateOrderDtoBuilder()
                .WithTableId(_fixture.TableId)
                .Build();

            var command = new StorefrontCreateOrderCommand(_fixture.MerchantId, createDto);
            var cancellationToken = CancellationToken.None;

            _orderCreationService
                .Setup(x => x.CreateOrAppendOrderAsync(It.IsAny<OrderCreationDto>(), cancellationToken))
                .ThrowsAsync(new NotFoundException("Bàn không tồn tại."));

            await Assert.ThrowsAsync<NotFoundException>(
                () => _handler.Handle(command, cancellationToken)
            );
        }

        [Fact]
        public async Task Handle_ServiceThrowsConflictException_ShouldPropagate()
        {
            var createDto = new StorefrontCreateOrderDtoBuilder()
                .WithTableId(_fixture.TableId)
                .Build();

            var command = new StorefrontCreateOrderCommand(_fixture.MerchantId, createDto);
            var cancellationToken = CancellationToken.None;

            _orderCreationService
                .Setup(x => x.CreateOrAppendOrderAsync(It.IsAny<OrderCreationDto>(), cancellationToken))
                .ThrowsAsync(new ConflictException("Bàn này đang có khách."));

            await Assert.ThrowsAsync<ConflictException>(
                () => _handler.Handle(command, cancellationToken)
            );
        }
    }
}
