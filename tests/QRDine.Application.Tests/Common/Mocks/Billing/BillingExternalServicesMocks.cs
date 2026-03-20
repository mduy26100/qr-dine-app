namespace QRDine.Application.Tests.Common.Mocks.Billing
{
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
