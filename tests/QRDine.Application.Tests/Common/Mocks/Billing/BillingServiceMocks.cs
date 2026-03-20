namespace QRDine.Application.Tests.Common.Mocks.Billing
{
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
}
