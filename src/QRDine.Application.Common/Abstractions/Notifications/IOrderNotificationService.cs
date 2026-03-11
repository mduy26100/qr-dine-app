namespace QRDine.Application.Common.Abstractions.Notifications
{
    public interface IOrderNotificationService
    {
        Task NotifyOrderUpdatedAsync(Guid merchantId, Guid tableId, string tableName, CancellationToken cancellationToken = default);
    }
}
