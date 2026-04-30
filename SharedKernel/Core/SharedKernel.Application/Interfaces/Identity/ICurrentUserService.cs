namespace SharedKernel.Application.Interfaces.Identity
{
    public interface ICurrentUserService
    {
        Guid UserId { get; }
        IEnumerable<string> Roles { get; }

        Guid? MerchantId { get; }

        bool IsAuthenticated { get; }
    }
}
