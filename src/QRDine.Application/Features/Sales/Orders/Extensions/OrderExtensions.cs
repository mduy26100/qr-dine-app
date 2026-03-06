using QRDine.Application.Features.Sales.Orders.DTOs;
using QRDine.Domain.Sales;
using System.Linq.Expressions;

namespace QRDine.Application.Features.Sales.Orders.Extensions
{
    public static class OrderExtensions
    {
        public static Expression<Func<Order, StorefrontOrderDto>> ToStorefrontOrderDto =>
            o => new StorefrontOrderDto
            {
                Id = o.Id,
                OrderCode = o.OrderCode,
                TableName = o.TableName,
                Status = o.Status.ToString(),
                TotalAmount = o.TotalAmount,
                Note = o.Note,
                Items = o.OrderItems.Where(oi => !oi.IsDeleted).Select(oi => new StorefrontOrderItemDto
                {
                    ProductId = oi.ProductId,
                    ProductName = oi.ProductName,
                    ImageUrl = oi.Product != null ? oi.Product.ImageUrl : null,
                    UnitPrice = oi.UnitPrice,
                    ToppingsSnapshot = oi.ToppingsSnapshot,
                    Quantity = oi.Quantity,
                    Amount = oi.Amount,
                    Note = oi.Note
                }).ToList()
            };
    }
}
