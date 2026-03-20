namespace QRDine.Application.Tests.Common.Fixtures
{
    public class BillingFixture : IDisposable
    {
        public readonly Guid MerchantId = Guid.NewGuid();
        public readonly Guid PlanId = Guid.NewGuid();
        public readonly Guid SubscriptionId = Guid.NewGuid();
        public readonly Guid TableId = Guid.NewGuid();
        public readonly Guid ProductId = Guid.NewGuid();

        public void Dispose()
        {
        }

        public void Reset()
        {
        }
    }
}
