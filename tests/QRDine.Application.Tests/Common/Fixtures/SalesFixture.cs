namespace QRDine.Application.Tests.Common.Fixtures
{
    public class SalesFixture : IDisposable
    {
        public readonly Guid MerchantId = Guid.NewGuid();
        public readonly Guid OrderId = Guid.NewGuid();
        public readonly Guid OrderItemId = Guid.NewGuid();
        public readonly Guid TableId = Guid.NewGuid();
        public readonly Guid SessionId = Guid.NewGuid();
        public readonly Guid ProductId = Guid.NewGuid();
        public readonly Guid ProductId2 = Guid.NewGuid();
        public readonly Guid ToppingId = Guid.NewGuid();

        public void Dispose()
        {
        }

        public void Reset()
        {
        }
    }
}
