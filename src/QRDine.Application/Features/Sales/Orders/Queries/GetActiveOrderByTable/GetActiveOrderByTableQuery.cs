using QRDine.Application.Features.Sales.Orders.DTOs;

namespace QRDine.Application.Features.Sales.Orders.Queries.GetActiveOrderByTable
{
    public record GetActiveOrderByTableQuery(Guid TableId) : IRequest<ManagementOrderDto?>;
}
