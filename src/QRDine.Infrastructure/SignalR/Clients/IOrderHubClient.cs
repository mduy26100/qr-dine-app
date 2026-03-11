namespace QRDine.Infrastructure.SignalR.Clients
{
    public interface IOrderHubClient
    {
        Task ReceiveOrderUpdate(Guid tableId, string message);
    }
}
