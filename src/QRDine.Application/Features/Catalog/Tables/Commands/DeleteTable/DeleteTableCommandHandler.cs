using QRDine.Application.Common.Exceptions;
using QRDine.Application.Features.Catalog.Repositories;

namespace QRDine.Application.Features.Catalog.Tables.Commands.DeleteTable
{
    public class DeleteTableCommandHandler : IRequestHandler<DeleteTableCommand>
    {
        private readonly ITableRepository _tableRepository;

        public DeleteTableCommandHandler(ITableRepository tableRepository)
        {
            _tableRepository = tableRepository;
        }

        public async Task Handle(DeleteTableCommand request, CancellationToken cancellationToken)
        {
            var table = await _tableRepository.GetByIdAsync(request.Id, cancellationToken);

            if (table == null)
            {
                throw new NotFoundException($"Table with ID {request.Id} not found.");
            }

            if (table.IsOccupied)
            {
                throw new ConflictException($"The table '{table.Name}' is currently occupied and cannot be deleted.");
            }

            table.IsDeleted = true;

            await _tableRepository.UpdateAsync(table, cancellationToken);
        }
    }
}
