namespace QRDine.Application.Tests.Common.Mocks.Sales
{
    public static class SalesRepositoryMocks
    {
        public static Mock<IOrderRepository> CreateOrderRepositoryMock()
        {
            return new Mock<IOrderRepository>();
        }

        public static Mock<IOrderItemRepository> CreateOrderItemRepositoryMock()
        {
            return new Mock<IOrderItemRepository>();
        }
    }
}
