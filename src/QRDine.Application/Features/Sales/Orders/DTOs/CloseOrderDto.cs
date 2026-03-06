using QRDine.Domain.Enums;

namespace QRDine.Application.Features.Sales.Orders.DTOs
{
    public class CloseOrderDto
    {
        public OrderStatus TargetStatus { get; set; }
    }
}
