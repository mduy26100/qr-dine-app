namespace QRDine.Application.Tests.Common.Mocks.Catalog
{
    public static class CatalogRepositoryMocks
    {
        public static Mock<ICategoryRepository> CreateCategoryRepositoryMock()
        {
            return new Mock<ICategoryRepository>();
        }

        public static Mock<IProductRepository> CreateProductRepositoryMock()
        {
            return new Mock<IProductRepository>();
        }

        public static Mock<ITableRepository> CreateTableRepositoryMock()
        {
            return new Mock<ITableRepository>();
        }

        public static Mock<IToppingGroupRepository> CreateToppingGroupRepositoryMock()
        {
            return new Mock<IToppingGroupRepository>();
        }

        public static Mock<IToppingRepository> CreateToppingRepositoryMock()
        {
            return new Mock<IToppingRepository>();
        }

        public static Mock<IProductToppingGroupRepository> CreateProductToppingGroupRepositoryMock()
        {
            return new Mock<IProductToppingGroupRepository>();
        }
    }
}
