namespace QRDine.Application.Tests.Features.Catalog.Tables.Commands.DeleteTable
{
    public class DeleteTableCommandHandlerTests
    {
        private readonly Mock<ITableRepository> _tableRepo;
        private readonly DeleteTableCommandHandler _handler;
        private readonly CatalogFixture _fixture;

        public DeleteTableCommandHandlerTests()
        {
            _fixture = new CatalogFixture();
            _tableRepo = CatalogRepositoryMocks.CreateTableRepositoryMock();
            _handler = new DeleteTableCommandHandler(_tableRepo.Object);
        }

        [Fact]
        public async Task Handle_TableNotExists_ShouldThrowNotFoundException()
        {
            var command = new DeleteTableCommand(_fixture.TableId);
            var cancellationToken = CancellationToken.None;

            _tableRepo
                .Setup(x => x.GetByIdAsync(_fixture.TableId, cancellationToken))
                .ReturnsAsync((Table?)null);

            await Assert.ThrowsAsync<NotFoundException>(
                () => _handler.Handle(command, cancellationToken)
            );
        }

        [Fact]
        public async Task Handle_TableIsOccupied_ShouldThrowConflictException()
        {
            var command = new DeleteTableCommand(_fixture.TableId);
            var cancellationToken = CancellationToken.None;

            var occupiedTable = new TableBuilder()
                .WithId(_fixture.TableId)
                .WithName("Table 1")
                .WithIsOccupied(true)
                .Build();

            _tableRepo
                .Setup(x => x.GetByIdAsync(_fixture.TableId, cancellationToken))
                .ReturnsAsync(occupiedTable);

            await Assert.ThrowsAsync<ConflictException>(
                () => _handler.Handle(command, cancellationToken)
            );
        }

        [Fact]
        public async Task Handle_ValidRequest_ShouldSoftDeleteTableSuccessfully()
        {
            var command = new DeleteTableCommand(_fixture.TableId);
            var cancellationToken = CancellationToken.None;

            var existingTable = new TableBuilder()
                .WithId(_fixture.TableId)
                .WithIsOccupied(false)
                .Build();

            _tableRepo
                .Setup(x => x.GetByIdAsync(_fixture.TableId, cancellationToken))
                .ReturnsAsync(existingTable);

            _tableRepo
                .Setup(x => x.UpdateAsync(existingTable, cancellationToken))
                .Returns(Task.CompletedTask);

            await _handler.Handle(command, cancellationToken);

            existingTable.IsDeleted.Should().BeTrue();
            _tableRepo.Verify(x => x.UpdateAsync(existingTable, cancellationToken), Times.Once);
        }
    }
}
