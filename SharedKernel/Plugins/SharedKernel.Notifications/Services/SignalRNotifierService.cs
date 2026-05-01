namespace SharedKernel.Notifications.Services
{
    public class SignalRNotifierService : IRealtimeNotifier
    {
        private readonly IHubContext<GenericNotificationHub> _hubContext;

        public SignalRNotifierService(IHubContext<GenericNotificationHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task SendToGroupAsync<T>(string groupName, string eventName, T payload, CancellationToken cancellationToken = default)
        {
            await _hubContext.Clients.Group(groupName).SendAsync(eventName, payload, cancellationToken);
        }

        public async Task SendToUserAsync<T>(string userId, string eventName, T payload, CancellationToken cancellationToken = default)
        {
            await _hubContext.Clients.User(userId).SendAsync(eventName, payload, cancellationToken);
        }

        public async Task SendToAllAsync<T>(string eventName, T payload, CancellationToken cancellationToken = default)
        {
            await _hubContext.Clients.All.SendAsync(eventName, payload, cancellationToken);
        }
    }
}
