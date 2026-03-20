namespace QRDine.Application.Tests.Common.Fixtures
{
    public class CatalogFixture : IDisposable
    {
        public readonly Guid MerchantId = Guid.NewGuid();
        public readonly Guid CategoryId = Guid.NewGuid();
        public readonly Guid ParentCategoryId = Guid.NewGuid();
        public readonly Guid ProductId = Guid.NewGuid();
        public readonly Guid TableId = Guid.NewGuid();
        public readonly Guid ToppingGroupId = Guid.NewGuid();
        public readonly Guid ToppingId = Guid.NewGuid();

        public void Dispose()
        {
        }

        public void Reset()
        {
        }
    }
}
