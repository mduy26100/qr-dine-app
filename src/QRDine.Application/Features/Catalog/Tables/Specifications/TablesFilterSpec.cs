using QRDine.Application.Features.Catalog.Tables.DTOs;
using QRDine.Domain.Catalog;

namespace QRDine.Application.Features.Catalog.Tables.Specifications
{
    public class TablesFilterSpec : Specification<Table, TableResponseDto>
    {
        public TablesFilterSpec(bool? isOccupied)
        {
            Query.Where(x => x.IsDeleted == false);

            if (isOccupied.HasValue)
            {
                Query.Where(x => x.IsOccupied == isOccupied.Value);
            }

            Query.OrderBy(x => x.Name);

            Query.Select(x => new TableResponseDto
            {
                Id = x.Id,
                Name = x.Name,
                IsOccupied = x.IsOccupied,
                QrCodeToken = x.QrCodeToken!,
                QrCodeImageUrl = x.QrCodeImageUrl!
            });
        }
    }
}
