namespace SharedKernel.Application.Interfaces.Notifications
{
    public interface IRealtimeNotifier
    {
        Task SendToGroupAsync<T>(string groupName, string eventName, T payload, CancellationToken cancellationToken = default);

        Task SendToUserAsync<T>(string userId, string eventName, T payload, CancellationToken cancellationToken = default);

        Task SendToAllAsync<T>(string eventName, T payload, CancellationToken cancellationToken = default);
    }
}
