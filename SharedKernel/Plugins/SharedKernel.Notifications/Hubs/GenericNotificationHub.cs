namespace SharedKernel.Notifications.Hubs
{
    [Authorize]
    public class GenericNotificationHub : Hub
    {
        private readonly IUserGroupProvider? _userGroupProvider;

        public GenericNotificationHub(IUserGroupProvider? userGroupProvider = null)
        {
            _userGroupProvider = userGroupProvider;
        }

        public override async Task OnConnectedAsync()
        {
            if (_userGroupProvider != null && Context.User != null)
            {
                var groups = await _userGroupProvider.GetUserGroupsAsync(Context.User);
                foreach (var group in groups)
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, group);
                }
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            if (_userGroupProvider != null && Context.User != null)
            {
                var groups = await _userGroupProvider.GetUserGroupsAsync(Context.User);
                foreach (var group in groups)
                {
                    await Groups.RemoveFromGroupAsync(Context.ConnectionId, group);
                }
            }

            await base.OnDisconnectedAsync(exception);
        }
    }
}
