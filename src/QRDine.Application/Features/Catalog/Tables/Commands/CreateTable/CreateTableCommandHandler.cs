using QRDine.Application.Common.Abstractions.ExternalServices.QrCode;
using QRDine.Application.Common.Abstractions.Identity;
using QRDine.Application.Common.Exceptions;
using QRDine.Application.Features.Catalog.Repositories;
using QRDine.Application.Features.Catalog.Tables.DTOs;
using QRDine.Application.Features.Catalog.Tables.Specifications;
using QRDine.Domain.Catalog;

namespace QRDine.Application.Features.Catalog.Tables.Commands.CreateTable
{
    public class CreateTableCommandHandler : IRequestHandler<CreateTableCommand, TableResponseDto>
    {
        private readonly ITableRepository _tableRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly ITableQrGeneratorService _tableQrGeneratorService;
        private readonly IMapper _mapper;

        public CreateTableCommandHandler(
            ITableRepository tableRepository,
            ICurrentUserService currentUserService,
            ITableQrGeneratorService tableQrGeneratorService,
            IMapper mapper)
        {
            _tableRepository = tableRepository;
            _currentUserService = currentUserService;
            _tableQrGeneratorService = tableQrGeneratorService;
            _mapper = mapper;
        }

        public async Task<TableResponseDto> Handle(CreateTableCommand request, CancellationToken cancellationToken)
        {
            var merchantId = _currentUserService.MerchantId
                ?? throw new UnauthorizedAccessException("Merchant context is missing.");

            var specName = new TableByNameSpec(request.Name, merchantId: merchantId, includeDeleted: true);
            var existingTable = await _tableRepository.FirstOrDefaultAsync(specName, cancellationToken);

            if (existingTable != null)
            {
                if (!existingTable.IsDeleted)
                {
                    throw new ConflictException($"The table name '{request.Name}' already exists in the system.");
                }

                existingTable.IsDeleted = false;
                existingTable.IsOccupied = false;

                await _tableRepository.UpdateAsync(existingTable, cancellationToken);

                return _mapper.Map<TableResponseDto>(existingTable);
            }

            var tableToken = Guid.NewGuid().ToString("N");
            var table = new Table
            {
                Name = request.Name,
                IsOccupied = false,
                QrCodeToken = tableToken
            };

            var uploadedUrl = await _tableQrGeneratorService.GenerateAndUploadQrAsync(
                merchantId,
                tableToken,
                table.Name,
                cancellationToken);

            table.QrCodeImageUrl = uploadedUrl;

            await _tableRepository.AddAsync(table, cancellationToken);

            return _mapper.Map<TableResponseDto>(table);
        }
    }
}