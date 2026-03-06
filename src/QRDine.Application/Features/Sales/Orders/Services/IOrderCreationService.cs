using QRDine.Application.Features.Sales.Orders.DTOs;
using QRDine.Domain.Sales;

namespace QRDine.Application.Features.Sales.Orders.Services
{
    public interface IOrderCreationService
    {
        Task<Order> CreateOrAppendOrderAsync(OrderCreationDto model, CancellationToken cancellationToken);
    }
}
