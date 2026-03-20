namespace QRDine.Application.Tests.Features.Sales.Orders.Services
{
    public class OrderCreationServiceTests
    {
        private readonly Mock<IOrderRepository> _orderRepository;
        private readonly Mock<IProductRepository> _productRepository;
        private readonly Mock<ITableRepository> _tableRepository;
        private readonly Mock<IApplicationDbContext> _dbContext;
        private readonly OrderCreationService _service;
        private readonly SalesFixture _fixture;

        public OrderCreationServiceTests()
        {
            _fixture = new SalesFixture();
            _orderRepository = SalesRepositoryMocks.CreateOrderRepositoryMock();
            _productRepository = new Mock<IProductRepository>();
            _tableRepository = new Mock<ITableRepository>();
            _dbContext = new Mock<IApplicationDbContext>();

            _service = new OrderCreationService(
                _orderRepository.Object,
                _productRepository.Object,
                _tableRepository.Object,
                _dbContext.Object
            );
        }

        [Fact]
        public async Task CreateOrAppendOrderAsync_TableNotExists_ShouldThrowNotFoundException()
        {
            var model = new OrderCreationDtoBuilder()
                .WithMerchantId(_fixture.MerchantId)
                .WithTableId(_fixture.TableId)
                .Build();

            var cancellationToken = CancellationToken.None;

            _tableRepository
                .Setup(x => x.GetByIdAsync(_fixture.TableId, cancellationToken))
                .ReturnsAsync((Table?)null);

            await Assert.ThrowsAsync<NotFoundException>(
                () => _service.CreateOrAppendOrderAsync(model, cancellationToken)
            );
        }

        [Fact]
        public async Task CreateOrAppendOrderAsync_TableMerchantIdMismatch_ShouldThrowNotFoundException()
        {
            var otherMerchantId = Guid.NewGuid();
            var table = new TableBuilder()
                .WithId(_fixture.TableId)
                .WithMerchantId(otherMerchantId)
                .Build();

            var model = new OrderCreationDtoBuilder()
                .WithMerchantId(_fixture.MerchantId)
                .WithTableId(_fixture.TableId)
                .Build();

            var cancellationToken = CancellationToken.None;

            _tableRepository
                .Setup(x => x.GetByIdAsync(_fixture.TableId, cancellationToken))
                .ReturnsAsync(table);

            await Assert.ThrowsAsync<NotFoundException>(
                () => _service.CreateOrAppendOrderAsync(model, cancellationToken)
            );
        }
    }
}
