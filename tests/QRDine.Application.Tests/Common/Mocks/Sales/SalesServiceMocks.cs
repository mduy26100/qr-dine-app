namespace QRDine.Application.Tests.Common.Mocks.Sales
{
    public static class SalesServiceMocks
    {
        public static Mock<IOrderCreationService> CreateOrderCreationServiceMock()
        {
            return new Mock<IOrderCreationService>();
        }

        public static Mock<IOrderFormattingService> CreateOrderFormattingServiceMock()
        {
            return new Mock<IOrderFormattingService>();
        }

        public static Mock<IOrderNotificationService> CreateOrderNotificationServiceMock()
        {
            return new Mock<IOrderNotificationService>();
        }

        public static Mock<IMapper> CreateMapperMock()
        {
            return new Mock<IMapper>();
        }
    }
}
