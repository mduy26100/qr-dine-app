namespace QRDine.Application.Features.Sales.Orders.Services
{
    public interface IOrderFormattingService
    {
        string? FormatToppingSnapshot(string? jsonSnapshot);
    }
}
