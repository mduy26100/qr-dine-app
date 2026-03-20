namespace QRDine.Application.Tests.Common.Mocks.Catalog
{
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
