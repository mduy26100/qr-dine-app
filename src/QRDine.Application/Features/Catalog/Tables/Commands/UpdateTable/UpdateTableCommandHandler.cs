using QRDine.Application.Common.Exceptions;
using QRDine.Application.Features.Catalog.Repositories;
using QRDine.Application.Features.Catalog.Tables.DTOs;
using QRDine.Application.Features.Catalog.Tables.Specifications;

namespace QRDine.Application.Features.Catalog.Tables.Commands.UpdateTable
{
    public class UpdateTableCommandHandler : IRequestHandler<UpdateTableCommand, TableResponseDto>
    {
        private readonly ITableRepository _tableRepository;
        private readonly IMapper _mapper;

        public UpdateTableCommandHandler(ITableRepository tableRepository, IMapper mapper)
        {
            _tableRepository = tableRepository;
            _mapper = mapper;
        }

        public async Task<TableResponseDto> Handle(UpdateTableCommand request, CancellationToken cancellationToken)
        {
            var table = await _tableRepository.GetByIdAsync(request.Id, cancellationToken);

            if (table == null)
            {
                throw new NotFoundException($"Table with ID {request.Id} not found.");
            }

            var specName = new TableByNameSpec(request.Dto.Name, request.Id);
            var isExists = await _tableRepository.AnyAsync(specName, cancellationToken);

            if (isExists)
            {
                throw new ConflictException($"The table name '{request.Dto.Name}' already exists in the system.");
            }

            table.Name = request.Dto.Name;

            await _tableRepository.UpdateAsync(table, cancellationToken);

            return _mapper.Map<TableResponseDto>(table);
        }
    }
}
