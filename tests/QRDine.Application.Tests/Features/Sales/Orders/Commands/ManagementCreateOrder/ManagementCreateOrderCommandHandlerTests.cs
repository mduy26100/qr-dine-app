namespace QRDine.Application.Tests.Features.Sales.Orders.Commands.ManagementCreateOrder
{
    public class ManagementCreateOrderCommandHandlerTests
    {
        private readonly Mock<IOrderCreationService> _orderCreationService;
        private readonly Mock<ITableRepository> _tableRepository;
        private readonly Mock<ICurrentUserService> _currentUserService;
        private readonly Mock<IMapper> _mapper;
        private readonly ManagementCreateOrderCommandHandler _handler;
        private readonly SalesFixture _fixture;

        public ManagementCreateOrderCommandHandlerTests()
        {
            _fixture = new SalesFixture();
            _orderCreationService = SalesServiceMocks.CreateOrderCreationServiceMock();
            _tableRepository = new Mock<ITableRepository>();
            _currentUserService = new Mock<ICurrentUserService>();
            _mapper = SalesServiceMocks.CreateMapperMock();

            _handler = new ManagementCreateOrderCommandHandler(
                _orderCreationService.Object,
                _tableRepository.Object,
                _currentUserService.Object,
                _mapper.Object
            );
        }

        [Fact]
        public async Task Handle_MerchantIdNotSet_ShouldThrowForbiddenException()
        {
            var createDto = new ManagementCreateOrderDtoBuilder()
                .WithTableId(_fixture.TableId)
                .Build();

            var command = new ManagementCreateOrderCommand(createDto);
            var cancellationToken = CancellationToken.None;

            _currentUserService
                .Setup(x => x.MerchantId)
                .Returns((Guid?)null);

            await Assert.ThrowsAsync<ForbiddenException>(
                () => _handler.Handle(command, cancellationToken)
            );
        }

        [Fact]
        public async Task Handle_ValidRequest_ShouldCreateOrderSuccessfully()
        {
            var createDto = new ManagementCreateOrderDtoBuilder()
                .WithTableId(_fixture.TableId)
                .Build();

            var command = new ManagementCreateOrderCommand(createDto);
            var cancellationToken = CancellationToken.None;

            var createdOrder = new OrderBuilder()
                .WithId(_fixture.OrderId)
                .WithMerchantId(_fixture.MerchantId)
                .WithTableId(_fixture.TableId)
                .WithSessionId(_fixture.SessionId)
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

            _currentUserService
                .Setup(x => x.MerchantId)
                .Returns(_fixture.MerchantId);

            _orderCreationService
                .Setup(x => x.CreateOrAppendOrderAsync(It.IsAny<OrderCreationDto>(), cancellationToken))
                .ReturnsAsync(createdOrder);

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
        public async Task Handle_ValidRequest_ShouldPassCorrectOrderCreationDto()
        {
            var tableId = Guid.NewGuid();
            var sessionId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var quantity = 2;

            var createDto = new ManagementCreateOrderDtoBuilder()
                .WithTableId(tableId)
                .WithSessionId(sessionId)
                .WithNote("Không cay")
                .WithItems(new List<ManagementCreateOrderItemDto>
                {
                    new ManagementCreateOrderItemDto
                    {
                        ProductId = productId,
                        Quantity = quantity,
                        Note = "Thêm muối",
                        SelectedToppingIds = new List<Guid>()
                    }
                })
                .Build();

            var command = new ManagementCreateOrderCommand(createDto);
            var cancellationToken = CancellationToken.None;

            var createdOrder = new OrderBuilder()
                .WithMerchantId(_fixture.MerchantId)
                .WithTableId(tableId)
                .WithSessionId(sessionId)
                .Build();

            _currentUserService
                .Setup(x => x.MerchantId)
                .Returns(_fixture.MerchantId);

            _orderCreationService
                .Setup(x => x.CreateOrAppendOrderAsync(It.IsAny<OrderCreationDto>(), cancellationToken))
                .ReturnsAsync(createdOrder);

            _mapper
                .Setup(x => x.Map<OrderResponseDto>(createdOrder))
                .Returns(new OrderResponseDto());

            await _handler.Handle(command, cancellationToken);

            _orderCreationService.Verify(
                x => x.CreateOrAppendOrderAsync(
                    It.Is<OrderCreationDto>(dto =>
                        dto.MerchantId == _fixture.MerchantId &&
                        dto.TableId == tableId &&
                        dto.SessionId == sessionId &&
                        dto.Note == "Không cay" &&
                        dto.Items.Count == 1 &&
                        dto.Items[0].ProductId == productId &&
                        dto.Items[0].Quantity == quantity
                    ),
                    cancellationToken
                ),
                Times.Once
            );
        }

        [Fact]
        public async Task Handle_ServiceThrowsNotFoundException_ShouldPropagate()
        {
            var createDto = new ManagementCreateOrderDtoBuilder()
                .WithTableId(_fixture.TableId)
                .Build();

            var command = new ManagementCreateOrderCommand(createDto);
            var cancellationToken = CancellationToken.None;

            _currentUserService
                .Setup(x => x.MerchantId)
                .Returns(_fixture.MerchantId);

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
            var createDto = new ManagementCreateOrderDtoBuilder()
                .WithTableId(_fixture.TableId)
                .Build();

            var command = new ManagementCreateOrderCommand(createDto);
            var cancellationToken = CancellationToken.None;

            _currentUserService
                .Setup(x => x.MerchantId)
                .Returns(_fixture.MerchantId);

            _orderCreationService
                .Setup(x => x.CreateOrAppendOrderAsync(It.IsAny<OrderCreationDto>(), cancellationToken))
                .ThrowsAsync(new ConflictException("Bàn này đang có khách."));

            await Assert.ThrowsAsync<ConflictException>(
                () => _handler.Handle(command, cancellationToken)
            );
        }
    }
}
