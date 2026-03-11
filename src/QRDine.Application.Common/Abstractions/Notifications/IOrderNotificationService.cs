namespace QRDine.Application.Common.Abstractions.Notifications
{
    public interface IOrderNotificationService
    {
        Task NotifyOrderUpdatedAsync(Guid merchantId, Guid tableId, CancellationToken cancellationToken = default);
    }
}
