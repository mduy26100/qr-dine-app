namespace SharedKernel.Application.Interfaces.Identity
{
    public interface IIdentityService
    {
        Task<int> CountStaffByMerchantAsync(Guid merchantId, CancellationToken cancellationToken);
    }
}
