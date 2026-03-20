using QRDine.Application.Features.Sales.Orders.DTOs;

namespace QRDine.Application.Features.Sales.Orders.Services
{
    public class OrderFormattingService : IOrderFormattingService
    {
        public string? FormatToppingSnapshot(string? jsonSnapshot)
        {
            if (string.IsNullOrWhiteSpace(jsonSnapshot))
                return null;

            try
            {
                var toppings = JsonSerializer.Deserialize<List<ToppingSnapshotItemDto>>(jsonSnapshot);
                if (toppings != null && toppings.Any())
                {
                    var formatted = toppings.Select(t => t.Price > 0 ? $"{t.Name} (+{t.Price:N0})" : t.Name);
                    return string.Join(", ", formatted);
                }
            }
            catch
            {
            }

            return null;
        }
    }
}
