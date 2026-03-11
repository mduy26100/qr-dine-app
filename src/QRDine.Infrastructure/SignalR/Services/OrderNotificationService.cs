using QRDine.Application.Common.Abstractions.Notifications;
using QRDine.Infrastructure.SignalR.Clients;
using QRDine.Infrastructure.SignalR.Hubs;

namespace QRDine.Infrastructure.SignalR.Services
{
    public class OrderNotificationService : IOrderNotificationService
    {
        private readonly IHubContext<OrderHub, IOrderHubClient> _hubContext;

        public OrderNotificationService(IHubContext<OrderHub, IOrderHubClient> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task NotifyOrderUpdatedAsync(Guid merchantId, Guid tableId, string tableName, CancellationToken cancellationToken = default)
        {
            var message = $"Bàn {tableName} vừa có cập nhật đơn hàng mới!";

            await _hubContext.Clients
                .Group($"Merchant_{merchantId}")
                .ReceiveOrderUpdate(tableId, tableName, message);
        }
    }
}
