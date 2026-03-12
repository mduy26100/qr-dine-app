using QRDine.Domain.Enums;

namespace QRDine.Application.Features.Dashboards.DTOs
{
    public class OrderMonthlySummaryDto
    {
        public OrderStatus Status { get; set; }
        public decimal TotalAmount { get; set; }
    }
}
