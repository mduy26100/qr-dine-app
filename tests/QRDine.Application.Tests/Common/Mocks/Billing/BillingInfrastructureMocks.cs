namespace QRDine.Application.Tests.Common.Mocks.Billing
{
    public static class BillingInfrastructureMocks
    {
        public static Mock<IApplicationDbContext> CreateApplicationDbContextMock()
        {
            return new Mock<IApplicationDbContext>();
        }

        public static Mock<IMapper> CreateMapperMock()
        {
            return new Mock<IMapper>();
        }
    }
}
