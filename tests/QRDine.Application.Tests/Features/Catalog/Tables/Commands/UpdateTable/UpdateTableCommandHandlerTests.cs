namespace QRDine.Application.Tests.Features.Catalog.Tables.Commands.UpdateTable
{
    public class UpdateTableCommandHandlerTests
    {
        private readonly Mock<ITableRepository> _tableRepo;
        private readonly Mock<IMapper> _mapper;
        private readonly UpdateTableCommandHandler _handler;
        private readonly CatalogFixture _fixture;

        public UpdateTableCommandHandlerTests()
        {
            _fixture = new CatalogFixture();
            _tableRepo = new Mock<ITableRepository>();
            _mapper = new Mock<IMapper>();

            _handler = new UpdateTableCommandHandler(_tableRepo.Object, _mapper.Object);
        }

        [Fact]
        public async Task Handle_TableNotExists_ShouldThrowNotFoundException()
        {
            var updateDto = new UpdateTableDtoBuilder().Build();
            var command = new UpdateTableCommand(_fixture.TableId, updateDto);

            _tableRepo
                .Setup(x => x.GetByIdAsync(_fixture.TableId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Table?)null);

            await Assert.ThrowsAsync<NotFoundException>(
                () => _handler.Handle(command, CancellationToken.None)
            );
        }

        [Fact]
        public async Task Handle_ValidRequest_ShouldUpdateTableSuccessfully()
        {
            var updateDto = new UpdateTableDtoBuilder()
                .WithName("Updated Table 1")
                .Build();

            var command = new UpdateTableCommand(_fixture.TableId, updateDto);

            var existingTable = new Table
            {
                Id = _fixture.TableId,
                Name = "Old Table 1",
                MerchantId = _fixture.MerchantId
            };

            var responseDto = new TableResponseDto
            {
                Id = _fixture.TableId,
                Name = "Updated Table 1"
            };

            _tableRepo
                .Setup(x => x.GetByIdAsync(_fixture.TableId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingTable);

            _tableRepo
                .Setup(x => x.AnyAsync(It.IsAny<ISpecification<Table>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            _tableRepo
                .Setup(x => x.UpdateAsync(existingTable, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _mapper
                .Setup(x => x.Map<TableResponseDto>(existingTable))
                .Returns(responseDto);

            var result = await _handler.Handle(command, CancellationToken.None);

            existingTable.Name.Should().Be("Updated Table 1");
            result.Name.Should().Be("Updated Table 1");
            _tableRepo.Verify(x => x.UpdateAsync(existingTable, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_DuplicateTableName_ShouldThrowConflictException()
        {
            var updateDto = new UpdateTableDtoBuilder()
                .WithName("Existing Table")
                .Build();

            var command = new UpdateTableCommand(_fixture.TableId, updateDto);

            var existingTable = new Table
            {
                Id = _fixture.TableId,
                Name = "Old Name",
                MerchantId = _fixture.MerchantId
            };

            _tableRepo
                .Setup(x => x.GetByIdAsync(_fixture.TableId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingTable);

            _tableRepo
                .Setup(x => x.AnyAsync(It.IsAny<ISpecification<Table>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            await Assert.ThrowsAsync<ConflictException>(
                () => _handler.Handle(command, CancellationToken.None)
            );
        }
    }
}