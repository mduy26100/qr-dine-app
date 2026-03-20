namespace QRDine.Application.Tests.Features.Catalog.Tables.Commands.CreateTable
{
    public class CreateTableCommandHandlerTests
    {
        private readonly Mock<ITableRepository> _tableRepo;
        private readonly Mock<ICurrentUserService> _currentUserService;
        private readonly Mock<ITableQrGeneratorService> _tableQrGeneratorService;
        private readonly Mock<IMapper> _mapper;
        private readonly CreateTableCommandHandler _handler;
        private readonly CatalogFixture _fixture;

        public CreateTableCommandHandlerTests()
        {
            _fixture = new CatalogFixture();
            _tableRepo = new Mock<ITableRepository>();
            _currentUserService = new Mock<ICurrentUserService>();
            _tableQrGeneratorService = new Mock<ITableQrGeneratorService>();
            _mapper = new Mock<IMapper>();

            _handler = new CreateTableCommandHandler(
                _tableRepo.Object,
                _currentUserService.Object,
                _tableQrGeneratorService.Object,
                _mapper.Object
            );
        }

        [Fact]
        public async Task Handle_MerchantIdMissing_ShouldThrowUnauthorizedAccessException()
        {
            var command = new CreateTableCommand("Table 1");
            _currentUserService.Setup(x => x.MerchantId).Returns((Guid?)null);

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                _handler.Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_DuplicateTableName_ShouldThrowConflictException()
        {
            var command = new CreateTableCommand("Table 1");
            var existingTable = new Table
            {
                Name = "Table 1",
                IsDeleted = false,
                MerchantId = _fixture.MerchantId
            };

            _currentUserService.Setup(x => x.MerchantId).Returns(_fixture.MerchantId);

            _tableRepo.Setup(x => x.FirstOrDefaultAsync(
                It.IsAny<ISpecification<Table>>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingTable);

            await Assert.ThrowsAsync<ConflictException>(() =>
                _handler.Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_DeletedTableSameName_ShouldRestoreTable()
        {
            var command = new CreateTableCommand("Table 1");
            var deletedTable = new Table
            {
                Id = _fixture.TableId,
                Name = "Table 1",
                IsDeleted = true,
                IsOccupied = true,
                MerchantId = _fixture.MerchantId
            };

            _currentUserService.Setup(x => x.MerchantId).Returns(_fixture.MerchantId);

            _tableRepo.Setup(x => x.FirstOrDefaultAsync(
                It.IsAny<ISpecification<Table>>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(deletedTable);

            _tableRepo.Setup(x => x.UpdateAsync(deletedTable, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _mapper.Setup(x => x.Map<TableResponseDto>(deletedTable))
                .Returns(new TableResponseDto { Id = _fixture.TableId, Name = "Table 1", IsOccupied = false });

            var result = await _handler.Handle(command, CancellationToken.None);

            deletedTable.IsDeleted.Should().BeFalse();
            deletedTable.IsOccupied.Should().BeFalse();
            _tableRepo.Verify(x => x.UpdateAsync(deletedTable, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ValidRequest_ShouldCreateTableSuccessfully()
        {
            var command = new CreateTableCommand("New Table");
            _currentUserService.Setup(x => x.MerchantId).Returns(_fixture.MerchantId);

            _tableRepo.Setup(x => x.FirstOrDefaultAsync(
                It.IsAny<ISpecification<Table>>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync((Table?)null);

            _tableQrGeneratorService.Setup(x => x.GenerateAndUploadQrAsync(
                _fixture.MerchantId, It.IsAny<string>(), "New Table", It.IsAny<CancellationToken>()))
                .ReturnsAsync("https://example.com/qr.png");

            _tableRepo.Setup(x => x.AddAsync(It.IsAny<Table>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Table t, CancellationToken ct) => t);

            _mapper.Setup(x => x.Map<TableResponseDto>(It.IsAny<Table>()))
                .Returns(new TableResponseDto { Name = "New Table" });

            var result = await _handler.Handle(command, CancellationToken.None);

            result.Should().NotBeNull();
            _tableRepo.Verify(x => x.AddAsync(It.IsAny<Table>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldGenerateQrCodeWithCorrectParameters()
        {
            var command = new CreateTableCommand("Table 1");
            _currentUserService.Setup(x => x.MerchantId).Returns(_fixture.MerchantId);

            _tableRepo.Setup(x => x.FirstOrDefaultAsync(
                It.IsAny<ISpecification<Table>>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync((Table?)null);

            await _handler.Handle(command, CancellationToken.None);

            _tableQrGeneratorService.Verify(
                x => x.GenerateAndUploadQrAsync(
                    _fixture.MerchantId,
                    It.Is<string>(s => !string.IsNullOrEmpty(s)),
                    "Table 1",
                    It.IsAny<CancellationToken>()),
                Times.Once
            );
        }
    }
}