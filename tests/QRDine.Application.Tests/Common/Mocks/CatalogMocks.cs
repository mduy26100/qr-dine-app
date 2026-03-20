namespace QRDine.Application.Tests.Common.Mocks
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

    public static class CatalogServiceMocks
    {
        public static Mock<ICurrentUserService> CreateCurrentUserServiceMock()
        {
            return new Mock<ICurrentUserService>();
        }

        public static Mock<IMapper> CreateMapperMock()
        {
            return new Mock<IMapper>();
        }

        public static Mock<IApplicationDbContext> CreateApplicationDbContextMock()
        {
            return new Mock<IApplicationDbContext>();
        }

        public static Mock<IFileUploadService> CreateFileUploadServiceMock()
        {
            return new Mock<IFileUploadService>();
        }

        public static Mock<ITableQrGeneratorService> CreateTableQrGeneratorServiceMock()
        {
            return new Mock<ITableQrGeneratorService>();
        }
    }
}
