namespace QRDine.Application.Tests.Common.Mocks.Billing
{
    public static class BillingRepositoryMocks
    {
        public static Mock<IPlanRepository> CreatePlanRepositoryMock()
        {
            return new Mock<IPlanRepository>();
        }

        public static Mock<ISubscriptionRepository> CreateSubscriptionRepositoryMock()
        {
            return new Mock<ISubscriptionRepository>();
        }

        public static Mock<ISubscriptionCheckoutRepository> CreateSubscriptionCheckoutRepositoryMock()
        {
            return new Mock<ISubscriptionCheckoutRepository>();
        }

        public static Mock<ITransactionRepository> CreateTransactionRepositoryMock()
        {
            return new Mock<ITransactionRepository>();
        }

        public static Mock<IFeatureLimitRepository> CreateFeatureLimitRepositoryMock()
        {
            return new Mock<IFeatureLimitRepository>();
        }

        public static Mock<ITableRepository> CreateTableRepositoryMock()
        {
            return new Mock<ITableRepository>();
        }

        public static Mock<IProductRepository> CreateProductRepositoryMock()
        {
            return new Mock<IProductRepository>();
        }

        public static Mock<IMerchantRepository> CreateMerchantRepositoryMock()
        {
            return new Mock<IMerchantRepository>();
        }
    }
}
