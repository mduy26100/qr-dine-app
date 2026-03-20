namespace QRDine.Application.Tests.Common.Mocks
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

    public static class BillingServiceMocks
    {
        public static Mock<ISubscriptionService> CreateSubscriptionServiceMock()
        {
            return new Mock<ISubscriptionService>();
        }

        public static Mock<IFeatureLimitService> CreateFeatureLimitServiceMock()
        {
            return new Mock<IFeatureLimitService>();
        }

        public static Mock<ICurrentUserService> CreateCurrentUserServiceMock()
        {
            return new Mock<ICurrentUserService>();
        }

        public static Mock<ICacheService> CreateCacheServiceMock()
        {
            return new Mock<ICacheService>();
        }

        public static Mock<IIdentityService> CreateIdentityServiceMock()
        {
            return new Mock<IIdentityService>();
        }
    }

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

    public static class BillingExternalServicesMocks
    {
        public static Mock<IPayOSService> CreatePayOSServiceMock()
        {
            return new Mock<IPayOSService>();
        }

        public static Mock<IFrontendConfig> CreateFrontendConfigMock()
        {
            return new Mock<IFrontendConfig>();
        }
    }
}
