namespace QRDine.Application.Common.Abstractions.Identity
{
    public interface IIdentityService
    {
        Task<int> CountStaffByMerchantAsync(Guid merchantId, CancellationToken cancellationToken);
    }
}
