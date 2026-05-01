namespace SharedKernel.Notifications.Providers
{
    public interface IUserGroupProvider
    {
        Task<IEnumerable<string>> GetUserGroupsAsync(ClaimsPrincipal user);
    }
}
