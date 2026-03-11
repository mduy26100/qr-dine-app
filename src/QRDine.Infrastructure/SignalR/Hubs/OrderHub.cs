using QRDine.Infrastructure.Identity.Constants;
using QRDine.Infrastructure.SignalR.Clients;

namespace QRDine.Infrastructure.SignalR.Hubs
{
    [Authorize]
    public class OrderHub : Hub<IOrderHubClient>
    {
        public override async Task OnConnectedAsync()
        {
            var merchantId = Context.User?.FindFirst(AppClaimTypes.MerchantId)?.Value;

            if (!string.IsNullOrWhiteSpace(merchantId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"Merchant_{merchantId}");
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var merchantId = Context.User?.FindFirst(AppClaimTypes.MerchantId)?.Value;

            if (!string.IsNullOrWhiteSpace(merchantId))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Merchant_{merchantId}");
            }

            await base.OnDisconnectedAsync(exception);
        }
    }
}
